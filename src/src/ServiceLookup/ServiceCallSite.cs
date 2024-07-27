// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup
{
    /// <summary>
    /// Summary description for ServiceCallSite
    /// </summary>
    internal abstract class ServiceCallSite(ResultCache cache)
    {
        public abstract Type         ServiceType        { get; }
        public abstract Type?        ImplementationType { get; }
        public abstract CallSiteKind Kind               { get; }
        public          ResultCache  Cache              { get; } = cache;
        public          object?      Value              { get; set; }
        public          object?      Key                { get; set; }

        public required ServiceResolvedHandler OnResolve { private get; init; }

        public void Resolved(object target) => OnResolve(ServiceType, target, (ServiceResolveKind)Kind);

        public bool CaptureDisposable =>
            ImplementationType == null ||
            typeof(IDisposable).IsAssignableFrom(ImplementationType) ||
            typeof(IAsyncDisposable).IsAssignableFrom(ImplementationType);
    }
}
