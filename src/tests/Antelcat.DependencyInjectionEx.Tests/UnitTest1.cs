using Microsoft.Extensions.DependencyInjection;

namespace Antelcat.DependencyInjectionEx.Tests;

public class Tests
{
    private ServiceProvider provider;
    
    [SetUp]
    public void Setup()
    {
        provider = new ServiceCollection()
            .AddSingleton(typeof(IA), typeof(A))
            .AddScoped(typeof(B))
            .AddTransient(typeof(C))
            .BuildServiceProviderEx();
        provider.ServiceResolved += (serviceType, instance, kind) =>
        {
            Console.WriteLine(instance.GetType());
        };
    }

    [Test]
    public void Test1()
    {
        Assert.NotNull(provider.GetRequiredService<B>());
        Assert.NotNull(provider.GetRequiredService<C>());
        var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<C>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<C>());
        var another = provider.CreateScope();
        Assert.NotNull(another.ServiceProvider.GetRequiredService<C>());
        Assert.NotNull(another.ServiceProvider.GetRequiredService<C>());
    }
    
    

    [TearDown]
    public void Dispose()
    {
        provider.Dispose();
    }
}

public interface IA;

public class A : IA;

public class B(IA a);

public class C(B b);