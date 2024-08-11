using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.DependencyInjectionEx.Autowired;

public static class AutowiredServiceProviderBuilderExtensions
{
    private static readonly ServiceResolveKind Kind = ServiceResolveKind.Constructor;
    private static ServiceProviderEx AutowiredProvider(this ServiceProviderEx serviceProvider)
    {
        serviceProvider.ServiceResolved += (provider, _, instance, kind) =>  Autowired.AutowiredProvider.Inject(instance, provider, kind);
        return serviceProvider;
    }


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProviderEx"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static ServiceProviderEx BuildAutowiredServiceProviderEx(this IServiceCollection collection) =>
        collection.BuildServiceProviderEx(new ServiceProviderOptions
        {
            ListenKind = Kind
        }).AutowiredProvider();


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProviderEx"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection, bool)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="validateScopes"></param>
    /// <returns></returns>
    public static ServiceProviderEx
        BuildAutowiredServiceProviderEx(this IServiceCollection collection, bool validateScopes) =>
        collection.BuildServiceProviderEx(new ServiceProviderOptions()
        {
            ListenKind     = Kind,
            ValidateScopes = validateScopes
        }).AutowiredProvider();


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProviderEx"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection, ServiceProviderOptions)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ServiceProviderEx BuildAutowiredServiceProviderEx(this IServiceCollection collection,
        ServiceProviderOptions options)
    {
        options.ListenKind |= ServiceResolveKind.Constructor; // can't be removed
        return collection.BuildServiceProviderEx(options).AutowiredProvider();
    }
}