// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class FactoryCallSite : ServiceCallSite
{
    public Func<IServiceProvider, object> Factory { get; }

    public FactoryCallSite(ResultCache cache,
        Func<CallSiteKind, bool> reportSelector,
        Type serviceType,
        Func<IServiceProvider, object> factory) : base(cache, reportSelector)
    {
        Factory     = factory;
        ServiceType = serviceType;
    }

    public FactoryCallSite(ResultCache cache, 
        Func<CallSiteKind, bool> reportSelector, 
        Type serviceType,
        object serviceKey,
        Func<IServiceProvider, object, object> factory) : base(cache, reportSelector)
    {
        Factory     = sp => factory(sp, serviceKey);
        ServiceType = serviceType;
    }

    public override Type  ServiceType        { get; }
    public override Type? ImplementationType => null;

    public override CallSiteKind Kind { get; } = CallSiteKind.Factory;
}