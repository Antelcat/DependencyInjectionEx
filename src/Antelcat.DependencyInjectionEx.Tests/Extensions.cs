using Microsoft.Extensions.DependencyInjection;
using Tests;

namespace Antelcat.DependencyInjectionEx.Tests;

public static class Extensions
{
    public static void TestResolve(this IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<D>().Check();
}