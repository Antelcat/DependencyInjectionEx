using System;

namespace Antelcat.DependencyInjectionEx.Callback;

internal abstract class ResolveTrigger
{
    public required IServiceProvider Provider { get; set; }
    
    public abstract void PostResolve(Type serviceType, object target, ServiceResolveKind kind);

    public abstract void FinishResolve();
}