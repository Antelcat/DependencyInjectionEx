// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup
{
    internal abstract class CallSiteVisitor<TArgument, TResult>
    {
        private readonly StackGuard _stackGuard = new();

        protected virtual TResult VisitCallSite(ServiceCallSite callSite, TArgument argument)
        {
            if (!_stackGuard.TryEnterOnCurrentStack())
            {
                return _stackGuard.RunOnEmptyStack(VisitCallSite, callSite, argument);
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

        protected virtual TResult VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
        {
            TResult ret;
            switch (callSite.Kind)
            {
                case CallSiteKind.Factory:
                    ret = VisitFactory((FactoryCallSite)callSite, argument);
                    break;
                case  CallSiteKind.IEnumerable:
                    ret = VisitIEnumerable((IEnumerableCallSite)callSite, argument);
                    break;
                case CallSiteKind.Constructor:
                    ret = VisitConstructor((ConstructorCallSite)callSite, argument);
                    break;
                case CallSiteKind.Constant:
                    ret = VisitConstant((ConstantCallSite)callSite, argument);
                    break;
                case CallSiteKind.ServiceProvider:
                    ret = VisitServiceProvider((ServiceProviderCallSite)callSite, argument);
                    break;
                default:
                    throw new NotSupportedException(SR.Format(SR.CallSiteTypeNotSupported, callSite.GetType()));
            }

            if (ret != null) callSite.Resolved(ret);
            return ret;
        }

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
}
