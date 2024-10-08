// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

[method: RequiresDynamicCode("Creates DynamicMethods")]
internal sealed class ILEmitServiceProviderEngine(ServiceProviderEx serviceProvider) : ServiceProviderEngine
{
    private readonly ILEmitResolverBuilder expressionResolverBuilder = new(serviceProvider);

    public override ServiceResolveHandler RealizeService(ServiceCallSite callSite) => 
        expressionResolverBuilder.Build(callSite);
}