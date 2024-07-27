using System;
using Antelcat.DependencyInjectionEx.ServiceLookup;

namespace Antelcat.DependencyInjectionEx;

public delegate void ServiceResolvedHandler(Type serviceType, object instance, CallSiteKind kind);
