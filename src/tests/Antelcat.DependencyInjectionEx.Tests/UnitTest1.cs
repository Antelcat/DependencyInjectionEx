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
            .AddScoped<IB, B>()
            .AddKeyedScoped(typeof(IB), nameof(IB), typeof(B))
            .AddTransient<IC, C>()
            .AddTransient(typeof(D))
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

    private int count;
    
    private void TestResolve(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<D>().Check(++count);
    }

    [Test]
    public async Task TestKeyedService()
    {
        TestResolve(provider);
        TestResolve(provider);
        TestResolve(provider);
        await Task.Delay(100);
        TestResolve(provider);
        TestResolve(provider);
        TestResolve(provider);
        var scope = provider.CreateScope().ServiceProvider;
        TestResolve(scope);
        TestResolve(scope);
        TestResolve(scope); 
        var another = provider.CreateScope().ServiceProvider;
        TestResolve(another);
        TestResolve(another);
        TestResolve(another); 
        TestResolve(another);
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

public interface IA;

public class A : IA
{
   [Autowired] public B C { get; set; }
}

public interface IB;

public class B(IA a) : IB;

public interface IC;
public class C(IB b) : IC
{
    [Autowired(typeof(IB), Key = nameof(IB))]
    public B B { get; set; }

    [Autowired(typeof(IB), Key = nameof(IB), GetServices = true)]
    public IEnumerable<IB> BS { get; set; }
}

public class D(IC c) : IDisposable
{
    [Autowired(typeof(IA))]
    public A A { get; set; }

    [Autowired(typeof(IB), Key = nameof(IB), GetServices = true)]
    public IEnumerable<IB> BS { get; set; }
    
    [Autowired]
    public IC C { get; set; }

    public void Check(int count)
    {
        Assert.NotNull(A);
        Assert.IsNotEmpty(BS);
        Assert.NotNull(C, $"{count} C is null");
    }

    public void Dispose()
    {
        // TODO 在此释放托管资源
    }
}