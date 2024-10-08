// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

[method: RequiresDynamicCode("Creates DynamicMethods")]
internal abstract class CompiledServiceProviderEngine(ServiceProviderEx providerEx) : ServiceProviderEngine
{
#if IL_EMIT
    public ILEmitResolverBuilder ResolverBuilder { get; } = new(providerEx);
#else
    public ExpressionResolverBuilder ResolverBuilder { get; } = new(providerEx);
#endif

    public override ServiceResolveHandler RealizeService(ServiceCallSite callSite) => ResolverBuilder.Build(callSite);
}