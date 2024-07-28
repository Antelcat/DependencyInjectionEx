using System.Runtime.CompilerServices;
using Antelcat.DependencyInjectionEx.Autowired;
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
            .AddKeyedScoped(typeof(IB), nameof(IB), typeof(B))
            .AddTransient(typeof(C))
            .BuildAutowiredServiceProviderEx();
    }

    [Test]
    public void TestService()
    {
        provider.GetRequiredService<B>();
        provider.GetRequiredService<C>();
        var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<C>();
        scope.ServiceProvider.GetRequiredService<C>();
        var another = provider.CreateScope();
        another.ServiceProvider.GetRequiredService<C>();
        another.ServiceProvider.GetRequiredService<C>();
    }

    [Test]
    public void TestKeyedService()
    {
        var c1 = provider.GetRequiredService<C>();
        Assert.NotNull(c1.B);
        Assert.NotNull(c1.BS);
        var scope = provider.CreateScope();
        var c2 = scope.ServiceProvider.GetRequiredService<C>();
        Assert.NotNull(c2.B);
        Assert.NotNull(c2.BS);
        var another = provider.CreateScope();
        var c3 = another.ServiceProvider.GetRequiredService<C>();
        Assert.NotNull(c3.B);
        Assert.NotNull(c3.BS);
    }



    [TearDown]
    public void Dispose()
    {
        provider.Dispose();
    }
}

public interface IA;

public class A : IA
{
    [Autowired] public B C { get; set; }
}

public interface IB;

public class B(IA a) : IB;


public class C(B b)
{
    [Autowired(typeof(IB), Key = nameof(IB))]
    public B B { get; set; }

    [Autowired(typeof(IB), Key = nameof(IB), GetServices = true)]
    public IEnumerable<IB> BS { get; set; }
}