// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class RuntimeServiceProviderEngine : ServiceProviderEngine
{
    public static RuntimeServiceProviderEngine Instance { get; } = new();

    private RuntimeServiceProviderEngine() { }

    public override ServiceResolveHandler RealizeService(ServiceCallSite callSite) => 
        scope => CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);
}