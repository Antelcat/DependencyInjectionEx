// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal abstract class CallSiteVisitor<TArgument, TResult>
{
    private readonly StackGuard stackGuard = new();

    protected virtual TResult VisitCallSite(ServiceCallSite callSite, TArgument argument)
    {
            if (!stackGuard.TryEnterOnCurrentStack())
            {
                return stackGuard.RunOnEmptyStack(VisitCallSite, callSite, argument);
            }

            switch (callSite.Cache.Location)
            {
                case CallSiteResultCacheLocation.Root:
                    return VisitRootCache(callSite, argument);
                case CallSiteResultCacheLocation.Scope:
                    return VisitScopeCache(callSite, argument);
                case CallSiteResultCacheLocation.Dispose:
                    return VisitDisposeCache(callSite, argument);
                case CallSiteResultCacheLocation.None:
                    return VisitNoCache(callSite, argument);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    protected virtual TResult VisitCallSiteMain(ServiceCallSite callSite, TArgument argument) =>
        callSite.Kind switch
        {
            CallSiteKind.Factory         => VisitFactory((FactoryCallSite)callSite, argument),
            CallSiteKind.IEnumerable     => VisitIEnumerable((IEnumerableCallSite)callSite, argument),
            CallSiteKind.Constructor     => VisitConstructor((ConstructorCallSite)callSite, argument),
            CallSiteKind.Constant        => VisitConstant((ConstantCallSite)callSite, argument),
            CallSiteKind.ServiceProvider => VisitServiceProvider((ServiceProviderCallSite)callSite, argument),
            _                            => throw new NotSupportedException(SR.Format(SR.CallSiteTypeNotSupported, callSite.GetType()))
        };

    protected virtual TResult VisitNoCache(ServiceCallSite callSite, TArgument argument)
    {
            return VisitCallSiteMain(callSite, argument);
        }

    protected virtual TResult VisitDisposeCache(ServiceCallSite callSite, TArgument argument)
    {
            return VisitCallSiteMain(callSite, argument);
        }

    protected virtual TResult VisitRootCache(ServiceCallSite callSite, TArgument argument)
    {
            return VisitCallSiteMain(callSite, argument);
        }

    protected virtual TResult VisitScopeCache(ServiceCallSite callSite, TArgument argument)
    {
            return VisitCallSiteMain(callSite, argument);
        }

    protected abstract TResult VisitConstructor(ConstructorCallSite constructorCallSite, TArgument argument);

    protected abstract TResult VisitConstant(ConstantCallSite constantCallSite, TArgument argument);

    protected abstract TResult VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, TArgument argument);

    protected abstract TResult VisitIEnumerable(IEnumerableCallSite enumerableCallSite, TArgument argument);

    protected abstract TResult VisitFactory(FactoryCallSite factoryCallSite, TArgument argument);
}