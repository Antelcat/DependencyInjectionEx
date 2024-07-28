using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.DependencyInjectionEx.Autowired;

public static class AutowiredServiceProviderBuilderExtensions
{
    private static ServiceProvider AutowiredProvider(this ServiceProvider serviceProvider)
    {
        var ap = new AutowiredProvider();
        serviceProvider.ServiceResolved += (provider, type, instance, kind) => ap.Inject(instance, provider, kind);
        return serviceProvider;
    }


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProvider"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static ServiceProvider BuildAutowiredServiceProviderEx(this IServiceCollection collection) =>
        collection.BuildServiceProviderEx().AutowiredProvider();


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProvider"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection, bool)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="validateScopes"></param>
    /// <returns></returns>
    public static ServiceProvider
        BuildAutowiredServiceProviderEx(this IServiceCollection collection, bool validateScopes) =>
        collection.BuildServiceProviderEx(validateScopes).AutowiredProvider();


    /// <summary>
    /// Extension methods for building a <see cref="ServiceProvider"/> from an <see cref="IServiceCollection"/>
    /// which resolves <see cref="AutowiredAttribute"/> annotated fields and properties.
    /// Calling <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProviderEx(IServiceCollection, ServiceProviderOptions)"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ServiceProvider BuildAutowiredServiceProviderEx(this IServiceCollection collection,
        ServiceProviderOptions options) =>
        collection.BuildServiceProviderEx(options).AutowiredProvider();

}