using System.Reflection;
using Antelcat.DependencyInjectionEx.Autowired;
using Tests;

namespace Antelcat.DependencyInjectionEx.Tests;

public class Service<T> : IDisposable
{
    private static int count;

    protected readonly int Number = ++count;

    public override string ToString() => $"{base.ToString()}-{Number}";

    public void Dispose()
    {
        Check();
        Console.WriteLine(this);
    }

    private static readonly IEnumerable<(string Name, Func<object, object?> Getter)> Autowires =
        typeof(T).GetProperties()
            .Where(static x => x is { CanRead: true, CanWrite: true })
            .Where(static x => x.GetCustomAttribute<AutowiredAttribute>() != null)
            .Select(static x => (x.Name, (Func<object, object?>)x.GetValue))
            .Concat(typeof(T).GetFields()
                .Where(static x => x.GetCustomAttribute<AutowiredAttribute>() != null)
                .Select(static x => (x.Name, (Func<object, object?>)x.GetValue)));

    [Autowired]
    public required IResolvable<T> Resolvable { get; set; }
    
    public void Check()
    {
        foreach (var autowire in Autowires) Assert.That(autowire.Getter(this), Is.Not.Null, $"{this}.{autowire.Name} is null");
    }
}