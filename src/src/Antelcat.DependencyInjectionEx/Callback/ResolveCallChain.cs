using System;
using Antelcat.DependencyInjectionEx.ServiceLookup;

namespace Antelcat.DependencyInjectionEx.Callback;

internal partial class ResolveCallChain(ResolveTrigger trigger)
{
    public object? RuntimePostResolve(object? resolved, ServiceCallSite callSite)
    {
        if (resolved is null) return null;
        var serviceType = callSite.ServiceType;
        var kind        = (ServiceResolveKind)callSite.Kind;
        trigger.PostResolve(serviceType, resolved, kind);
        return resolved;
    }

    public void OnResolved(IServiceProvider provider) => trigger.FinishResolve(provider);
}