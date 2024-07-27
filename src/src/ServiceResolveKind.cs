namespace Antelcat.DependencyInjectionEx;

public enum ServiceResolveKind
{
    /// <summary>
    /// Indicates that the service was resolved by a factory.
    /// </summary>
    Factory,

    /// <summary>
    /// Indicates that the service was resolved by a constructor.
    /// </summary>
    Constructor,

    /// <summary>
    /// Indicates that the service was resolved as a constant.
    /// </summary>
    Constant,

    /// <summary>
    /// Indicates that the service was resolved as an enumerable.
    /// </summary>
    IEnumerable,

    /// <summary>
    /// Indicates that the service was resolved by a service provider.
    /// </summary>
    ServiceProvider,
}