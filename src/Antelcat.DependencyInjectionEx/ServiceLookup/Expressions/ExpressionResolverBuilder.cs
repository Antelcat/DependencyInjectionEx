// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Antelcat.DependencyInjectionEx.Callback;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ExpressionResolverBuilder : CallSiteVisitor<object?, Expression>
{
    private static readonly ParameterExpression ScopeParameter =
        Expression.Parameter(typeof(ServiceProviderEngineScopeWrap));

    private static readonly ParameterExpression ResolvedServices =
        Expression.Variable(typeof(IDictionary<ServiceCacheKey, object>), ScopeParameter.Name + "resolvedServices");

    private static readonly ParameterExpression Sync = 
        Expression.Variable(typeof(object), ScopeParameter.Name + "sync");

    private static readonly ParameterExpression CallChain =
        Expression.Variable(typeof(ResolveCallChain), ScopeParameter.Name + "callChain");

    private static readonly BinaryExpression ResolvedServicesVariableAssignment =
        Expression.Assign(ResolvedServices,
            Expression.Property(
                ScopeParameter,
                typeof(ServiceProviderEngineScopeWrap).GetProperty(nameof(ServiceProviderEngineScopeWrap.ResolvedServices),
                    BindingFlags.Instance | BindingFlags.Public)!));

    private static readonly BinaryExpression SyncVariableAssignment =
        Expression.Assign(Sync,
            Expression.Property(
                ScopeParameter,
                typeof(ServiceProviderEngineScopeWrap).GetProperty(nameof(ServiceProviderEngineScopeWrap.Sync),
                    BindingFlags.Instance | BindingFlags.Public)!));

    private static readonly BinaryExpression CallChainVariableAssignment =
        Expression.Assign(CallChain,
            Expression.Property(ScopeParameter, ServiceLookupHelpers.CallChain));
    
    private static readonly ParameterExpression CaptureDisposableParameter = Expression.Parameter(typeof(object));

    private static readonly LambdaExpression CaptureDisposable = Expression.Lambda(
        Expression.Call(ScopeParameter, ServiceLookupHelpers.CaptureDisposableMethodInfo, CaptureDisposableParameter),
        CaptureDisposableParameter);

    private static readonly ConstantExpression CallSiteRuntimeResolverInstanceExpression = Expression.Constant(
        CallSiteRuntimeResolver.Instance,
        typeof(CallSiteRuntimeResolver));
    
    
    private readonly ServiceProviderEngineScope rootScope;

    private readonly ConcurrentDictionary<ServiceCacheKey, ServiceResolveHandler> scopeResolverCache;

    private readonly Func<ServiceCacheKey, ServiceCallSite, ServiceResolveHandler> buildTypeDelegate;

    public ExpressionResolverBuilder(ServiceProviderEx serviceProvider)
    {
        rootScope          = serviceProvider.Root;
        scopeResolverCache = new ConcurrentDictionary<ServiceCacheKey, ServiceResolveHandler>();
        buildTypeDelegate  = (_, cs) => BuildNoCache(cs);
    }

    public ServiceResolveHandler Build(ServiceCallSite callSite)
    {
        // Only scope methods are cached
        if (callSite.Cache.Location == CallSiteResultCacheLocation.Scope)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            return scopeResolverCache.GetOrAdd(callSite.Cache.Key, key => buildTypeDelegate(key, callSite));
#else
            return scopeResolverCache.GetOrAdd(callSite.Cache.Key, buildTypeDelegate, callSite);
#endif
        }

        return BuildNoCache(callSite);
    }

    public ServiceResolveHandler BuildNoCache(ServiceCallSite callSite)
    {
        var expression = BuildExpression(callSite);
        DependencyInjectionEventSource.Log.ExpressionTreeGenerated(rootScope.RootProviderEx, callSite.ServiceType,
            expression);
        return expression.Compile();
    }

    private Expression<ServiceResolveHandler> BuildExpression(ServiceCallSite callSite) =>
        callSite.Cache.Location == CallSiteResultCacheLocation.Scope
            ? Expression.Lambda<ServiceResolveHandler>(
                Expression.Block(
                    [
                        ResolvedServices, Sync, CallChain
                    ],
                    ResolvedServicesVariableAssignment,
                    SyncVariableAssignment,
                    CallChainVariableAssignment,
                    BuildScopedExpression(callSite)),
                ScopeParameter)
            : Expression.Lambda<ServiceResolveHandler>(
                Expression.Block(
                    [CallChain],
                    CallChainVariableAssignment,
                    Convert(VisitCallSite(callSite, null), typeof(object), forceValueTypeConversion: true)
                ),
                ScopeParameter);

    protected override Expression VisitRootCache(ServiceCallSite singletonCallSite, object? context) => 
        Expression.Constant(CallSiteRuntimeResolver.Instance.Resolve(singletonCallSite, rootScope));

    protected override Expression VisitConstant(ConstantCallSite constantCallSite, object? context) => 
        Expression.Constant(constantCallSite.DefaultValue);

    protected override Expression VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object? context) => 
        ScopeParameter;

    protected override Expression VisitFactory(FactoryCallSite factoryCallSite, object? context) => 
        Expression.Invoke(Expression.Constant(factoryCallSite.Factory), ScopeParameter);

    protected override Expression VisitIEnumerable(IEnumerableCallSite callSite, object? context)
    {
        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "VerifyAotCompatibility ensures elementType is not a ValueType")]
        static MethodInfo GetArrayEmptyMethodInfo(Type elementType)
        {
            Debug.Assert(!ServiceProviderEx.VerifyAotCompatibility || !elementType.IsValueType,
                "VerifyAotCompatibility=true will throw during building the IEnumerableCallSite if elementType is a ValueType.");

            return ServiceLookupHelpers.GetArrayEmptyMethodInfo(elementType);
        }

        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "VerifyAotCompatibility ensures elementType is not a ValueType")]
        static NewArrayExpression NewArrayInit(Type elementType, IEnumerable<Expression> expr)
        {
            Debug.Assert(!ServiceProviderEx.VerifyAotCompatibility || !elementType.IsValueType,
                "VerifyAotCompatibility=true will throw during building the IEnumerableCallSite if elementType is a ValueType.");

            return Expression.NewArrayInit(elementType, expr);
        }

        if (callSite.ServiceCallSites.Length == 0)
            return Expression.Constant(
                GetArrayEmptyMethodInfo(callSite.ItemType)
                    .Invoke(obj: null, parameters: []));

        return NewArrayInit(
            callSite.ItemType,
            callSite.ServiceCallSites.Select(cs =>
                Convert(
                    VisitCallSite(cs, context),
                    callSite.ItemType)));
    }

    protected override Expression VisitDisposeCache(ServiceCallSite callSite, object? context) =>
        // Elide calls to GetCaptureDisposable if the implementation type isn't disposable
        TryCaptureDisposable(
            callSite,
            ScopeParameter,
            VisitCallSiteMain(callSite, context));

    private static Expression TryCaptureDisposable(ServiceCallSite callSite, ParameterExpression scope,
        Expression service) =>
        !callSite.CaptureDisposable ? service : Expression.Invoke(GetCaptureDisposable(scope), service);

    protected override Expression VisitConstructor(ConstructorCallSite callSite, object? context)
    {
        ParameterInfo[] parameters = callSite.ConstructorInfo.GetParameters();
        Expression[]    parameterExpressions;
        if (callSite.ParameterCallSites.Length == 0)
        {
            parameterExpressions = [];
        }
        else
        {
            parameterExpressions = new Expression[callSite.ParameterCallSites.Length];
            for (int i = 0; i < parameterExpressions.Length; i++)
            {
                parameterExpressions[i] = Convert(VisitCallSite(callSite.ParameterCallSites[i], context),
                    parameters[i].ParameterType);
            }
        }

        Expression expression = Expression.New(callSite.ConstructorInfo, parameterExpressions);
        if (callSite.ImplementationType!.IsValueType) expression = Expression.Convert(expression, typeof(object));

        return expression;
    }

    protected override Expression VisitCallback(Expression result, ServiceCallSite callSite, object? context)
    {
        if (callSite.NeedReport)
        {
            var callChain = CallChain;
            return Expression.Call(callChain, ServiceLookupHelpers.PostResolve, result, Expression.Constant(callSite));
        }
        return result;
    }

    private static Expression Convert(Expression expression, Type type, bool forceValueTypeConversion = false) =>
        // Don't convert if the expression is already assignable
        type.IsAssignableFrom(expression.Type) && (!expression.Type.IsValueType || !forceValueTypeConversion)
            ? expression
            : Expression.Convert(expression, type);

    protected override Expression VisitScopeCache(ServiceCallSite callSite, object? context)
    {
        ServiceResolveHandler lambda = Build(callSite);
        return Expression.Invoke(Expression.Constant(lambda), ScopeParameter);
    }

    // Move off the main stack
    private ConditionalExpression BuildScopedExpression(ServiceCallSite callSite)
    {
        ConstantExpression callSiteExpression = Expression.Constant(
            callSite,
            typeof(ServiceCallSite));

        // We want to directly use the callsite value if it's set and the scope is the root scope.
        // We've already called into the RuntimeResolver and pre-computed any singletons or root scope
        // Avoid the compilation for singletons (or promoted singletons)
        MethodCallExpression resolveRootScopeExpression = Expression.Call(
            CallSiteRuntimeResolverInstanceExpression,
            ServiceLookupHelpers.ResolveCallSiteAndScopeMethodInfo,
            callSiteExpression,
            ScopeParameter);

        ConstantExpression keyExpression = Expression.Constant(
            callSite.Cache.Key,
            typeof(ServiceCacheKey));

        ParameterExpression resolvedVariable = Expression.Variable(typeof(object), "resolved");

        ParameterExpression resolvedServices = ResolvedServices;

        MethodCallExpression tryGetValueExpression = Expression.Call(
            resolvedServices,
            ServiceLookupHelpers.TryGetValueMethodInfo,
            keyExpression,
            resolvedVariable);

        Expression captureDisposible =
            TryCaptureDisposable(callSite, ScopeParameter, VisitCallSiteMain(callSite, null));

        BinaryExpression assignExpression = Expression.Assign(
            resolvedVariable,
            captureDisposible);

        MethodCallExpression addValueExpression = Expression.Call(
            resolvedServices,
            ServiceLookupHelpers.AddMethodInfo,
            keyExpression,
            resolvedVariable);

        BlockExpression blockExpression = Expression.Block(
            typeof(object),
            new[]
            {
                resolvedVariable
            },
            Expression.IfThen(
                Expression.Not(tryGetValueExpression),
                Expression.Block(
                    assignExpression,
                    addValueExpression)),
            resolvedVariable);


        // The C# compiler would copy the lock object to guard against mutation.
        // We don't, since we know the lock object is readonly.
        ParameterExpression lockWasTaken = Expression.Variable(typeof(bool), "lockWasTaken");
        ParameterExpression sync         = Sync;

        MethodCallExpression monitorEnter =
            Expression.Call(ServiceLookupHelpers.MonitorEnterMethodInfo, sync, lockWasTaken);
        MethodCallExpression monitorExit = Expression.Call(ServiceLookupHelpers.MonitorExitMethodInfo, sync);

        BlockExpression       tryBody     = Expression.Block(monitorEnter, blockExpression);
        ConditionalExpression finallyBody = Expression.IfThen(lockWasTaken, monitorExit);

        return Expression.Condition(
            Expression.Property(
                ScopeParameter,
                typeof(IServiceProviderEngineScope)
                    .GetProperty(nameof(IServiceProviderEngineScope.IsRootScope),
                        BindingFlags.Instance | BindingFlags.Public)!),
            resolveRootScopeExpression,
            Expression.Block(
                typeof(object),
                new[] { lockWasTaken },
                Expression.TryFinally(tryBody, finallyBody))
        );
    }

    public static Expression GetCaptureDisposable(ParameterExpression scope) =>
        scope != ScopeParameter
            ? throw new NotSupportedException(SR.GetCaptureDisposableNotSupported)
            : CaptureDisposable;
}