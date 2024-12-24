using Antelcat.DependencyInjectionEx.Autowired;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests;

namespace Antelcat.DependencyInjectionEx.Tests;

public class Tests
{
    private ServiceProviderEx provider;
    private ServiceProviderEx keyedProvider;
    
    [SetUp]
    public void Setup()
    {
        provider = new ServiceCollection()
            .AddTransient(typeof(IResolvable<>), typeof(Resolvable<>))
            .AddSingleton(typeof(IA), typeof(A))
            .AddSingleton<IB, B>()
            .AddTransient<IC, C>()
            .AddTransient(typeof(D))
            .BuildAutowiredServiceProviderEx();
        provider.ServiceResolved += (_, _, instance, kind) =>
        {
            Console.WriteLine($"{kind} {instance}");
        };

        keyedProvider = new ServiceCollection()
            .AddKeyedSingleton(typeof(IA), nameof(IA), typeof(KeyA))
            .AddKeyedScoped(typeof(IB), nameof(IB), typeof(KeyB))
            .AddKeyedTransient(typeof(IC), nameof(IC), typeof(KeyC))
            .BuildAutowiredServiceProviderEx();
    }

    [Test]
    public void TestOnce() => provider.CreateScope().ServiceProvider.GetService<IA>();

    [Test]
    public async Task TestTribe()
    {
        provider.CreateScope().ServiceProvider.TestResolve();
        provider.CreateScope().ServiceProvider.TestResolve();
        await Task.Delay(100);
        provider.CreateScope().ServiceProvider.TestResolve();
    }

    [Test]
    public async Task TestService()
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
    public async Task TestKeyedService()
    {
        var root = keyedProvider.CreateScope().ServiceProvider;
        root.GetRequiredKeyedService<IC>(nameof(IC));
        var scope = keyedProvider.CreateScope().ServiceProvider;
        scope.GetRequiredKeyedService<IC>(nameof(IC));
        var another = keyedProvider.CreateScope().ServiceProvider;
        another.GetRequiredKeyedService<IC>(nameof(IC));
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
        keyedProvider.Dispose();
    }
}
