using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Antelcat.DependencyInjectionEx.Autowired;

public class AutowiredProvider
{
    private readonly ConcurrentDictionary<Type, AutowiredResolver> resolvers = [];
    private readonly HashSet<Type>                                 ignores   = [];

    public void Inject(object target, IServiceProvider provider, ServiceResolveKind kind)
    {
        if (kind is not ServiceResolveKind.Constructor) return;
        var type = target.GetType();
        if (ignores.Contains(type)) return;
        if (!resolvers.TryGetValue(type, out var resolver))
        {
            resolver = new AutowiredResolver(type);
            if (!resolver.NeedResolve)
            {
                ignores.Add(type);
                return;
            }
            resolvers.TryAdd(type, resolver);
        }

        resolver.Map(target, provider);
    }
}