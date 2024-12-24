using Antelcat.DependencyInjectionEx.Autowired;
using Antelcat.DependencyInjectionEx.Tests;
using Microsoft.Extensions.DependencyInjection;


// ReSharper disable once CheckNamespace
namespace Tests;

public interface IA : IDisposable;

public class A : Service<A>, IA
{
    [Autowired]
    public required IB B { get; set; }
}

public interface IB : IDisposable;

public class B : Service<B>, IB
{
    [Autowired]
    public required IA A { get; set; }
}

public interface IC : IDisposable;

public class C(IA a,IB b) : Service<C>, IC;

public class D(IA a, IB b, IC c) : Service<D>;

public interface IResolvable<T> : IDisposable;

public class Resolvable<T> : IResolvable<T>
{
    public Resolvable()
    {
        Console.WriteLine("Ctor : " + this);
    }
    
    private static int count;

    protected readonly int Number = ++count;

    public override string ToString() => $"{base.ToString()}-{Number}";

    public void Dispose() => Console.WriteLine("Dctor : " + this);
}



public class KeyA : Resolvable<IA>, IA
{
    
}

public class KeyB([FromKeyedServices(nameof(IA))] IA a) : Resolvable<IB>, IB
{

}

public class KeyC([FromKeyedServices(nameof(IA))] IA a, [FromKeyedServices(nameof(IB))] IB b) : Resolvable<IC>, IC
{

}