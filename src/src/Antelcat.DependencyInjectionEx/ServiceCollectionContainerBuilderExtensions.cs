// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Antelcat.DependencyInjectionEx;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for building a <see cref="ServiceProviderEx"/> from an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionContainerBuilderExtensions
{
    /// <summary>
    /// Creates a <see cref="ServiceProviderEx"/> containing services from the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <returns>The <see cref="ServiceProviderEx"/>.</returns>
    public static ServiceProviderEx BuildServiceProviderEx(this IServiceCollection services) => 
        BuildServiceProviderEx(services, ServiceProviderOptions.Default);

    /// <summary>
    /// Creates a <see cref="ServiceProviderEx"/> containing services from the provided <see cref="IServiceCollection"/>
    /// optionally enabling scope validation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <param name="validateScopes">
    /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
    /// </param>
    /// <returns>The <see cref="ServiceProviderEx"/>.</returns>
    public static ServiceProviderEx BuildServiceProviderEx(this IServiceCollection services, bool validateScopes) => 
        services.BuildServiceProviderEx(new ServiceProviderOptions { ValidateScopes = validateScopes });

    /// <summary>
    /// Creates a <see cref="ServiceProviderEx"/> containing services from the provided <see cref="IServiceCollection"/>
    /// optionally enabling scope validation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <param name="options">
    /// Configures various service provider behaviors.
    /// </param>
    /// <returns>The <see cref="ServiceProviderEx"/>.</returns>
    public static ServiceProviderEx BuildServiceProviderEx(this IServiceCollection services, ServiceProviderOptions options)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (options is null) throw new ArgumentNullException(nameof(options));
        return new ServiceProviderEx(services, options);
    }
}