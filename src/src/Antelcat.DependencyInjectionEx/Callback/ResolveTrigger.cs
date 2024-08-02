using System;

namespace Antelcat.DependencyInjectionEx.Callback;

public abstract class ResolveTrigger
{
    public IServiceProvider? Provider { get; set; }
    
    public abstract void PostResolve(Type serviceType, object target, ServiceResolveKind kind);

    public abstract void FinishResolve();
}