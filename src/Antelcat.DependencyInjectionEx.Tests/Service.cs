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

    private static readonly IEnumerable<Action<object>> Checks =
        typeof(T).GetProperties()
            .Where(static x => x is { CanRead: true, CanWrite: true })
            .Where(static x => x.GetCustomAttribute<AutowiredAttribute>() != null)
            .Select(static x => new Action<object>(target =>
            {
                Assert.That(x.GetValue(target), Is.Not.Null, $"{target}.{x.Name} is null");
            }))
            .Concat(typeof(T).GetFields()
                .Where(static x => x.GetCustomAttribute<AutowiredAttribute>() != null)
                .Select(static x => new Action<object>(target =>
                {
                    Assert.That(x.GetValue(target), Is.Not.Null, $"{target}.{x.Name} is null");
                })));

    [Autowired]
    public required IResolvable<T> Resolvable { get; set; }
    
    public void Check()
    {
        foreach (var check in Checks) check(this);
    }
}