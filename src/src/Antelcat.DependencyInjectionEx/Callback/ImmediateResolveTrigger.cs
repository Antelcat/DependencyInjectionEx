using System;

namespace Antelcat.DependencyInjectionEx.Callback;

internal class ImmediateResolveTrigger(ServiceResolvedHandler handler) : ResolveTrigger
{
    public override void PostResolve(Type serviceType, object target, ServiceResolveKind kind) => 
        handler(Provider, serviceType, target, kind);

    public override void FinishResolve() { }
}