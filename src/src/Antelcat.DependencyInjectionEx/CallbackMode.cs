using System;

namespace Antelcat.DependencyInjectionEx;

public enum CallbackMode
{
    /// <summary>
    /// <see cref="ServiceProvider"/>.<see cref="ServiceProvider.ServiceResolved"/> be triggered whenever a service is resolved
    /// </summary>
    Each,
    /// <summary>
    /// <see cref="ServiceProvider"/>.<see cref="ServiceProvider.ServiceResolved"/> will be triggered after calling <see cref="IServiceProvider.GetService"/>
    /// </summary>
    Batch
}