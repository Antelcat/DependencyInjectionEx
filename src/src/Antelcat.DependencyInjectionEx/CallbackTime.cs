using System;

namespace Antelcat.DependencyInjectionEx;

public enum CallbackTime
{
    /// <summary>
    /// <see cref="ServiceProviderEx"/>.<see cref="ServiceProviderEx.ServiceConstructed"/> will be triggered before <see cref="IServiceProvider.GetService"/> returned
    /// </summary>
    Finally,

    /// <summary>
    /// <see cref="ServiceProviderEx"/>.<see cref="ServiceProviderEx.ServiceConstructed"/> be triggered after a service is constructed
    /// </summary>
    Immediately,
}