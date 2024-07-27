using System;

namespace Antelcat.DependencyInjectionEx;

public delegate void ServiceResolvedHandler(Type serviceType, object instance, ServiceResolveKind kind);
