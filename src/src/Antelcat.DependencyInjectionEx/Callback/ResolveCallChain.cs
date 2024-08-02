using System;
using System.Diagnostics.CodeAnalysis;
using Antelcat.DependencyInjectionEx.ServiceLookup;

namespace Antelcat.DependencyInjectionEx.Callback;

internal class ResolveCallChain(ResolveTrigger trigger)
{
    public IServiceProvider? Provider
    {
        get => trigger.Provider;
        set => trigger.Provider = value;
    }

    [return: NotNullIfNotNull(nameof(resolved))]
    public object? PostResolve(object? resolved, ServiceCallSite callSite)
    {
        if (resolved is null) return null;
        var serviceType = callSite.ServiceType;
        var kind        = (ServiceResolveKind)callSite.Kind;
        trigger.PostResolve(serviceType, resolved, kind);
        return resolved;
    }

    public void OnResolved() => trigger.FinishResolve();
}