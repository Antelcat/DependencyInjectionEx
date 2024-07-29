using System;

namespace Antelcat.DependencyInjectionEx.Callback;

public abstract class ResolveTrigger
{
    public abstract void PostResolve(Type serviceType, object target, ServiceResolveKind kind);

    public abstract void FinishResolve(IServiceProvider provider);
}