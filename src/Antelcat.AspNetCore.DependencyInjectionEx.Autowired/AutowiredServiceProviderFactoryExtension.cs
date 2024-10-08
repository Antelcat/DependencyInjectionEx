using Antelcat.AspNetCore.DependencyInjectionEx.Autowired;
using Antelcat.DependencyInjectionEx;
using Antelcat.DependencyInjectionEx.Autowired;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AutowiredServiceProviderFactoryExtension
{
    
    /// <summary>
    /// Build <see cref="AutowiredServiceProviderFactory"/> for <see cref="AutowiredAttribute"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder UseAutowiredServiceProviderFactory(
        this IHostBuilder builder) =>
        builder.UseServiceProviderFactory(new AutowiredServiceProviderFactory(x => x.BuildAutowiredServiceProviderEx()));
    
    /// <summary>
    /// Build <see cref="AutowiredServiceProviderFactory"/> for <see cref="AutowiredAttribute"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="validateScopes"></param>
    /// <returns></returns>
    public static IHostBuilder UseAutowiredServiceProviderFactory(
        this IHostBuilder builder,
        bool validateScopes) =>
        builder.UseServiceProviderFactory(new AutowiredServiceProviderFactory(x => x.BuildAutowiredServiceProviderEx(validateScopes)));

    /// <summary>
    /// Build <see cref="AutowiredServiceProviderFactory"/> for <see cref="AutowiredAttribute"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder UseAutowiredServiceProviderFactory(
        this IHostBuilder builder,
        ServiceProviderOptions options) =>
        builder.UseServiceProviderFactory(new AutowiredServiceProviderFactory(x => x.BuildAutowiredServiceProviderEx(options)));

}