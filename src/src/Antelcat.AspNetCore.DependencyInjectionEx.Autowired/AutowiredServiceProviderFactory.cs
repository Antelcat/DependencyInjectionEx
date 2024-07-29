using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.AspNetCore.DependencyInjectionEx.Autowired;

public class AutowiredServiceProviderFactory(Func<IServiceCollection,IServiceProvider> build) : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder) => build(containerBuilder);
}