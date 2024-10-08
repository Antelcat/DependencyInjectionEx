// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx;

/// <summary>
/// Options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.
/// </summary>
public class ServiceProviderOptions
{
    // Avoid allocating objects in the default case
    internal static readonly ServiceProviderOptions Default = new();

    /// <summary>
    /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>. Defaults to <c>false</c>.
    /// </summary>
    public bool ValidateScopes { get; set; }

    /// <summary>
    /// <c>true</c> to perform check verifying that all services can be created during <c>BuildServiceProvider</c> call; otherwise <c>false</c>. Defaults to <c>false</c>.
    /// NOTE: this check doesn't verify open generics services.
    /// </summary>
    public bool ValidateOnBuild { get; set; }

    /// <summary>
    /// Determined kind of service resolve when listening to <see cref="ServiceProviderEx.ServiceResolved"/> event. Default is <see cref="ServiceResolveKind.None"/>.
    /// </summary>
    public ServiceResolveKind ListenKind { get; set; } = ServiceResolveKind.None;
    
    /// <summary>
    /// Determined when to trigger <see cref="ServiceProviderEx"/>.<see cref="ServiceProviderEx.ServiceResolved"/>. Default is <see cref="CallbackTime.Finally"/>.
    /// </summary>
    public CallbackTime CallbackTime { get; set; } = CallbackTime.Finally;
}