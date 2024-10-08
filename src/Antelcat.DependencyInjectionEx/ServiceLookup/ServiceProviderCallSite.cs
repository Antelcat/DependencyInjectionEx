// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ServiceProviderCallSite(Func<CallSiteKind, bool> reportSelector)
    : ServiceCallSite(ResultCache.None(typeof(IServiceProvider)), reportSelector)
{
    public override Type         ServiceType        { get; } = typeof(IServiceProvider);
    public override Type         ImplementationType { get; } = typeof(ServiceProviderEx);
    public override CallSiteKind Kind               { get; } = CallSiteKind.ServiceProvider;
}