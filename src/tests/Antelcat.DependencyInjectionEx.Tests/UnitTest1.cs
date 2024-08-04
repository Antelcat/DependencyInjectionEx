using Antelcat.DependencyInjectionEx.Autowired;
using Microsoft.Extensions.DependencyInjection;
using Tests;

namespace Antelcat.DependencyInjectionEx.Tests;

public class Tests
{
    private ServiceProviderEx provider;
    
    [SetUp]
    public void Setup()
    {
        provider = new ServiceCollection()
            .AddTransient(typeof(IResolvable<>), typeof(Resolvable<>))
            .AddSingleton(typeof(IA), typeof(A))
            .AddScoped<IB, B>()
            .AddTransient<IC, C>()
            .AddTransient(typeof(D))
            .BuildAutowiredServiceProviderEx(new ServiceProviderOptions
            {
                CallbackTime = CallbackTime.Finally,
            });
        provider.ServiceConstructed += (_, _, instance, kind) =>
        {
            Console.WriteLine($"{kind} {instance}");
        };
    }

    [Test]
    public void TestOnce() => provider.CreateScope().ServiceProvider.TestResolve();

    [Test]
    public async Task TestTribe()
    {
        provider.CreateScope().ServiceProvider.TestResolve();
        provider.CreateScope().ServiceProvider.TestResolve();
        await Task.Delay(100);
        provider.CreateScope().ServiceProvider.TestResolve();
    }

    [Test]
    public async Task TestKeyedService()
    {
        var root = provider.CreateScope().ServiceProvider;
        root.TestResolve();
        await Task.Delay(300);
        root.TestResolve();
        provider.TestResolve();
        provider.TestResolve();
        var scope = provider.CreateScope().ServiceProvider;
        scope.TestResolve();
        scope.TestResolve();
        var another = provider.CreateScope().ServiceProvider;
        another.TestResolve();
        another.TestResolve();
    }
    
    [Test]
    public void TestCacheWeave()
    {
        var a = provider.GetRequiredService<IA>();
        var a2 = provider.GetRequiredService<IA>();
        var a3 = provider.GetRequiredService<IA>();
        provider.GetRequiredService<IA>();
        provider.GetRequiredService<IA>();
    }
    


    [TearDown]
    public void Dispose()
    {
        provider.Dispose();
    }
}
