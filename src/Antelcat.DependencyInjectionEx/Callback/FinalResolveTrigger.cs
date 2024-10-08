using System;

namespace Antelcat.DependencyInjectionEx.Callback;

internal class FinalResolveTrigger(ServiceResolvedHandler handler) : ResolveTrigger
{
    private event Action<IServiceProvider>? Resolves;

    public override void PostResolve(Type serviceType, object target, ServiceResolveKind kind) =>
        Resolves += provider => handler(provider, serviceType, target, kind);

    public override void FinishResolve()
    {
        if (Resolves == null) return;
        var resolves = Resolves;
        Resolves = null;
        resolves?.Invoke(Provider);
    }
}