using Antelcat.DependencyInjectionEx.Autowired;

namespace Antelcat.DependencyInjectionEx.TestWeb;

public interface IA;

public class A : IA
{
    
}

public interface IB;

public class B : IB
{
    [Autowired]
    public required IA A { get; set; }
}