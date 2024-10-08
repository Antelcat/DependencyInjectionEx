// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.DependencyInjectionEx;

/// <summary>
/// Default implementation of <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
/// </summary>
public class DefaultServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly ServiceProviderOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceProviderFactory"/> class
    /// with default options.
    /// </summary>
    public DefaultServiceProviderFactory() : this(ServiceProviderOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceProviderFactory"/> class
    /// with the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The options to use for this instance.</param>
    public DefaultServiceProviderFactory(ServiceProviderOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    /// <inheritdoc />
    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return containerBuilder.BuildServiceProviderEx(options);
    }
}