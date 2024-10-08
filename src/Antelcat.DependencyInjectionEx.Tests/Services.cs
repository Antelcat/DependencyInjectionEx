using Antelcat.DependencyInjectionEx.Autowired;
using Antelcat.DependencyInjectionEx.Tests;



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
    private static int count;

    protected readonly int Number = ++count;

    public override string ToString() => $"{base.ToString()}-{Number}";

    public void Dispose() => Console.WriteLine(this);
}