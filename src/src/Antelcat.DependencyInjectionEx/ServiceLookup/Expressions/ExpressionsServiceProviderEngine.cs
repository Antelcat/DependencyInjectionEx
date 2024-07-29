// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ExpressionsServiceProviderEngine(ServiceProvider serviceProvider) : ServiceProviderEngine
{
    private readonly ExpressionResolverBuilder expressionResolverBuilder = new(serviceProvider);

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
    {
            return expressionResolverBuilder.Build(callSite);
        }
}