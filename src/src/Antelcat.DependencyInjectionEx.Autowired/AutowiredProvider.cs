using System;
using System.Collections.Concurrent;

namespace Antelcat.DependencyInjectionEx.Autowired;

public static class AutowiredProvider
{
    private static readonly ConcurrentDictionary<Type, AutowiredResolver> Resolvers = [];

    public static void Inject(object target, IServiceProvider provider, ServiceResolveKind kind)
    {
        if (kind is not ServiceResolveKind.Constructor || target is Type) return;
        var type = target.GetType();
        if (SystemType(type)) return;
        Resolvers.GetOrAdd(type, static key => new AutowiredResolver(key)).Map(target, provider);
    }

    private static bool SystemType(Type type) =>
        type.IsEnum      ||
        type.IsPointer   ||
        type.IsPrimitive ||
        type.IsArray     ||
        type.IsImport    ||
        type.IsCOMObject;
}