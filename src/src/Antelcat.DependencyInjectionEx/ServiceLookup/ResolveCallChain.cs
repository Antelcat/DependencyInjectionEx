using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal partial class ResolveCallChain(ServiceResolvedHandler resolvedHandler)
{
    public object? RuntimePostResolve(object? resolved, ServiceCallSite callSite)
    {
        if (resolved is null) return null;
        var serviceType = callSite.ServiceType;
        var kind        = (ServiceResolveKind)callSite.Kind;
        Resolves += provider => resolvedHandler(provider, serviceType, resolved, kind);
        return resolved;
    }

    private event Action<IServiceProvider>? Resolves;
    public void OnResolved(IServiceProvider provider) => Resolves?.Invoke(provider);
}