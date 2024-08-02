using System;
using System.Diagnostics;

namespace Antelcat.DependencyInjectionEx.Callback;

public class EachResolveTrigger(ServiceResolvedHandler handler) : ResolveTrigger
{
    public override void PostResolve(Type serviceType, object target, ServiceResolveKind kind)
    {
        if(Provider is not null) handler(Provider, serviceType, target, kind);
        else
        {
            Debugger.Break();
        }
    }

    public override void FinishResolve() { }
}