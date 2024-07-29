using System;

namespace Antelcat.DependencyInjectionEx.Callback;

public class EachResolveTrigger(ServiceResolvedHandler handler, IServiceProvider provider) : ResolveTrigger
{
    public override void PostResolve(Type serviceType, object target, ServiceResolveKind kind)
    {
        handler(provider, serviceType, target, kind);
    }

    public override void FinishResolve(IServiceProvider _) { }
}