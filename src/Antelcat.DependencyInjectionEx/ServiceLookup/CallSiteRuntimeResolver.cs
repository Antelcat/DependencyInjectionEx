// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Antelcat.DependencyInjectionEx.Callback;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class CallSiteRuntimeResolver : CallSiteVisitor<RuntimeResolverContext, object?>
{
    public static CallSiteRuntimeResolver Instance { get; } = new();

    private CallSiteRuntimeResolver() { }

    public object? Resolve(ServiceCallSite callSite, IServiceProviderEngineScope scope)
    {
        // Fast path to avoid virtual calls if we already have the cached value in the root scope
        if (scope.IsRootScope && callSite.Value is { } cached) return cached;

        return VisitCallSite(callSite, new RuntimeResolverContext
        {
            Scope     = scope,
            CallChain = (scope as ServiceProviderEngineScopeWrap)?.CallChain
        });
    }

    protected override object? VisitDisposeCache(ServiceCallSite transientCallSite, RuntimeResolverContext context) => 
        context.Scope.CaptureDisposable(VisitCallSiteMain(transientCallSite, context));

    protected override object? VisitCallback(object? result, ServiceCallSite callSite, RuntimeResolverContext argument) =>
        callSite.NeedReport && argument.CallChain is not null
            ? argument.CallChain.PostResolve(result, callSite)
            : result;

    protected override object VisitConstructor(ConstructorCallSite constructorCallSite, RuntimeResolverContext context)
    {
        object?[] parameterValues;
        if (constructorCallSite.ParameterCallSites.Length == 0)
        {
            parameterValues = [];
        }
        else
        {
            parameterValues = new object?[constructorCallSite.ParameterCallSites.Length];
            for (int index = 0; index < parameterValues.Length; index++)
            {
                parameterValues[index] = VisitCallSite(constructorCallSite.ParameterCallSites[index], context);
            }
        }


#if NETFRAMEWORK || NETSTANDARD2_0
        try
        {
            return constructorCallSite.ConstructorInfo.Invoke(parameterValues);
        }
        catch (Exception ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            // The above line will always throw, but the compiler requires we throw explicitly.
            throw;
        }
#else
        return constructorCallSite.ConstructorInfo.Invoke(BindingFlags.DoNotWrapExceptions,
            binder: null,
            parameters: parameterValues,
            culture: null);
#endif
    }

    protected override object? VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
    {
        if (callSite.Value is { } value)
        {
            // Value already calculated, return it directly
            return value;
        }

        const RuntimeResolverLock   lockType              = RuntimeResolverLock.Root;
        IServiceProviderEngineScope serviceProviderEngine = context.Scope.RootProviderEx.Root;

        lock (callSite)
        {
            // Lock the callsite and check if another thread already cached the value
            if (callSite.Value is { } callSiteValue)
            {
                return callSiteValue;
            }

            var rc = new RuntimeResolverContext
            {
                Scope         = serviceProviderEngine,
                AcquiredLocks = context.AcquiredLocks | lockType,
                CallChain     = context.CallChain
            };
            object? resolved = VisitCallSiteMain(callSite, rc);
            serviceProviderEngine.CaptureDisposable(resolved);
            callSite.Value = resolved;
            return resolved;
        }
    }

    protected override object? VisitScopeCache(ServiceCallSite callSite, RuntimeResolverContext context)
    {
        // Check if we are in the situation where scoped service was promoted to singleton
        // and we need to lock the root
        return context.Scope.IsRootScope ?
            VisitRootCache(callSite, context) :
            VisitCache(callSite, context, context.Scope, RuntimeResolverLock.Scope);
    }

    private object? VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, IServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
    {
        bool                                 lockTaken        = false;
        object                               sync             = serviceProviderEngine.Sync;
        Dictionary<ServiceCacheKey, object?> resolvedServices = serviceProviderEngine.ResolvedServices;
        // Taking locks only once allows us to fork resolution process
        // on another thread without causing the deadlock because we
        // always know that we are going to wait the other thread to finish before
        // releasing the lock
        if ((context.AcquiredLocks & lockType) == 0)
        {
            Monitor.Enter(sync, ref lockTaken);
        }

        try
        {
            // Note: This method has already taken lock by the caller for resolution and access synchronization.
            // For scoped: takes a dictionary as both a resolution lock and a dictionary access lock.
            if (resolvedServices.TryGetValue(callSite.Cache.Key, out object? resolved))
            {
                return resolved;
            }

            var rc = new RuntimeResolverContext
            {
                Scope         = serviceProviderEngine,
                AcquiredLocks = context.AcquiredLocks | lockType,
                CallChain     = context.CallChain
            };
            resolved         =  VisitCallSiteMain(callSite, rc);
            serviceProviderEngine.CaptureDisposable(resolved);
            resolvedServices.Add(callSite.Cache.Key, resolved);
            return resolved;
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(sync);
            }
        }
    }

    protected override object? VisitConstant(ConstantCallSite constantCallSite, RuntimeResolverContext context) => 
        constantCallSite.DefaultValue;

    protected override object VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, RuntimeResolverContext context) => 
        context.Scope;

    protected override object VisitIEnumerable(IEnumerableCallSite enumerableCallSite, RuntimeResolverContext context)
    {
        Array array = CreateArray(
            enumerableCallSite.ItemType,
            enumerableCallSite.ServiceCallSites.Length);

        for (int index = 0; index < enumerableCallSite.ServiceCallSites.Length; index++)
        {
            object? value = VisitCallSite(enumerableCallSite.ServiceCallSites[index], context);
            array.SetValue(value, index);
        }
        return array;
        
        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "VerifyAotCompatibility ensures elementType is not a ValueType")]
        static Array CreateArray(Type elementType, int length)
        {
            Debug.Assert(!ServiceProviderEx.VerifyAotCompatibility || !elementType.IsValueType, "VerifyAotCompatibility=true will throw during building the IEnumerableCallSite if elementType is a ValueType.");

            return Array.CreateInstance(elementType, length);
        }
    }

    protected override object VisitFactory(FactoryCallSite factoryCallSite, RuntimeResolverContext context) => 
        factoryCallSite.Factory(context.Scope);
}

internal readonly struct RuntimeResolverContext
{
    public IServiceProviderEngineScope Scope { get; init; }

    public RuntimeResolverLock AcquiredLocks { get; init; }

    public required ResolveCallChain? CallChain { get; init; }
}

[Flags]
internal enum RuntimeResolverLock
{
    Scope = 1,
    Root  = 2
}