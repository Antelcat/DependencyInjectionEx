using System;

namespace Antelcat.DependencyInjectionEx.Autowired;

/// <summary>
/// Property or field marked with this attribute will be automatically injected by the container.
/// </summary>
/// <param name="serviceType"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class AutowiredAttribute(Type? serviceType = null) : Attribute
{
    internal Type? ServiceType => serviceType;

    public object? Key { get; set; }

    public bool GetServices { get; set; }
}
