using System;

namespace Antelcat.DependencyInjectionEx;

[Flags]
public enum ServiceResolveKind
{
    /// <summary>
    /// Nothing happens
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Indicates that the service was resolved by a factory.
    /// </summary>
    Factory = 0x1,

    /// <summary>
    /// Indicates that the service was resolved by a constructor.
    /// </summary>
    Constructor = 0x2,

    /// <summary>
    /// Indicates that the service was resolved as a constant.
    /// </summary>
    Constant = 0x4,

    /// <summary>
    /// Indicates that the service was resolved as an enumerable.
    /// </summary>
    IEnumerable = 0x8,

    /// <summary>
    /// Indicates that the service was resolved as a service provider.
    /// </summary>
    ServiceProvider = 0x10,
}