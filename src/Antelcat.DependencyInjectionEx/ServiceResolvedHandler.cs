using System;

namespace Antelcat.DependencyInjectionEx;

public delegate void ServiceResolvedHandler(IServiceProvider provider,
    Type serviceType,
    object instance,
    ServiceResolveKind kind);
