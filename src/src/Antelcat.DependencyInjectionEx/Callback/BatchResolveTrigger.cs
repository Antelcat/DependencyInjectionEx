using System;

namespace Antelcat.DependencyInjectionEx.Callback;

public class BatchResolveTrigger(ServiceResolvedHandler handler) : ResolveTrigger
{
    private event Action<IServiceProvider>? Resolves;

    public override void PostResolve(Type serviceType, object target, ServiceResolveKind kind) => Resolves += provider => handler(provider, serviceType, target, kind);

    public override void FinishResolve(IServiceProvider provider) => Resolves?.Invoke(provider);
}