// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal abstract class ServiceProviderEngine
{
    public abstract ServiceResolveHandler RealizeService(ServiceCallSite callSite);
}