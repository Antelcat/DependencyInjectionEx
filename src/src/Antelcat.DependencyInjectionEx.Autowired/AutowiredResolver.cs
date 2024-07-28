using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Antelcat.IL;
using Antelcat.IL.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.DependencyInjectionEx.Autowired;

internal class AutowiredResolver(Type type)
{
    internal static bool IL =>
#if NETFRAMEWORK || NETSTANDARD2_0
        true;
#else
            RuntimeFeature.IsDynamicCodeSupported;
#endif

    public bool NeedResolve => mappers.Count > 0;

    private readonly IList<Action<object, IServiceProvider>> mappers = type
        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Select(static x =>
            new Tuple<FieldInfo, AutowiredAttribute?>(x, x.GetCustomAttribute<AutowiredAttribute>()))
        .Where(static x => x.Item2 != null)
        .Select(static x => IL
            ? MapHandler(x.Item1.CreateSetter(), x.Item2!, x.Item1.FieldType)
            : MapReflection(x.Item1.SetValue, x.Item2!, x.Item1.FieldType))
        .Concat(type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(static x =>
                new Tuple<PropertyInfo, AutowiredAttribute?>(x, x.GetCustomAttribute<AutowiredAttribute>()))
            .Where(static x => x.Item1.CanWrite && x.Item2 != null)
            .Select(static x => IL
                ? MapHandler(x.Item1.CreateSetter(), x.Item2!, x.Item1.PropertyType)
                : MapReflection(x.Item1.SetValue, x.Item2!, x.Item1.PropertyType)))
        .ToArray();

    private static Action<object, IServiceProvider> MapReflection(
        Action<object, object> setter,
        AutowiredAttribute attribute,
        Type memberType)
    {
        var key  = attribute.Key;
        var type = attribute.ServiceType ?? memberType;
        return key is null
            ? (target, provider) =>
            {
                var value = provider.GetService(type);
                if (value is not null) setter(target!, value);
            }
            : !attribute.GetServices
                ? (target, provider) => { setter(target!, provider.GetRequiredKeyedService(type, key)); }
                : (target, provider) => { setter(target!, provider.GetKeyedServices(type, key)); };
    }

    private static Action<object, IServiceProvider> MapHandler(
        SetHandler<object, object> handler,
        AutowiredAttribute attribute,
        Type memberType)
    {
        var key  = attribute.Key;
        var type = attribute.ServiceType ?? memberType;

        return key is null
            ? (target, provider) =>
            {
                var value = provider.GetService(type);
                if (value is not null) handler(ref target!, value);
            }
            : !attribute.GetServices
                ? (target, provider) => { handler(ref target!, provider.GetRequiredKeyedService(type, key)); }
                : (target, provider) => { handler(ref target!, provider.GetKeyedServices(type, key)); };
    }

    public void Map(object target, IServiceProvider provider)
    {
        foreach (var mapper in mappers) mapper(target, provider);
    }
}