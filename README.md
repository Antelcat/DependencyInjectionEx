# Antelcat.DependencyInjectionEx

Rebuild from [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

## Antelcat.DependencyInjectionEx

Export service resolve callback

### `BuildServiceProviderEx(this IServiceCollection, ...)`

```csharp
class ServiceProviderEx : IServiceProvider;

ServiceProviderEx provider = new ServiceCollection()
                                .Add...(...)
                                ...
                                .BuildServiceProviderEx(options...);
```

### [`ServiceProviderOptions`](./src/src/Antelcat.DependencyInjectionEx/ServiceProviderOptions.cs)

+ #### [`ListenKind`](./src/src/Antelcat.DependencyInjectionEx/ServiceResolveKind.cs) indicates which kind of behavior to be listen

    ```csharp
    enum ServiceResolvedKind{
        None =             0, // nothing
        Factory =          1, // service resolved from Func<...> 
        Constructor =      2, // service resolved by ctor
        Constant =         4, // service resolved from registered instance
        IEnumerable =      8, // service required by IEnumerable<>
        ServiceProvider = 16, // require IServiceProvider
    }
    ```

+ #### `CallbackTime` indicates when should the `ServiceResolved` be triggered

    ```csharp
    public enum CallbackTime
    {
        Finally,     // ServiceResolved will be triggered before GetService() returned
        Immediately, // ServiceResolved will be triggered after each instance resolved
    }
    ```
  + `Finally` Mode

    resolve A => resolve B => resolve C => trigger A => trigger B => trigger C

    >   finally mode can resolve circular dependency

  + `Immediately` Mode

    resolve A => trigger A => resolve B => trigger B => resolve C => trigger C

    >   immediately mode can let dependency full-filled before injected


### [`ServiceProviderEx.ServiceResolved`](./src/src/Antelcat.DependencyInjectionEx/ServiceResolvedHandler.cs) event to be triggered when service resolved

```csharp
delegate void ServiceResolvedHandler(
    IServiceProvider provider, // service provider which resolved this service
    Type serviceType,          // required service type
    object instance,           // resolved instance
    ServiceResolveKind kind    // resolved kind
    );

provider.ServiceResolved += (provider, serviceType, instance, kind) => {
    // do something
};
```

---

## Antelcat.DependencyInjectionEx.Autowired.Abstraction

### [`AutowiredAttribute`](./src/src/Antelcat.DependencyInjectionEx.Autowired.Abstractions/AutowiredAttribute.cs)

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class AutowiredAttribute(Type? serviceType = null) : Attribute
{
    public object? Key { get; set; } // key for keyed service

    public bool GetServices { get; set; } // IEnumerable<...>
}
```

Usage

```csharp
class Service{
    [Autowired]
    private IService? service;
    
    [Autowired(typeof(IService))]
    public Service service { get; init; }
    
    [Autowired(typeof(IService), Key="string key", GetServices=true)]
    public IEnumerable<Service> keyedService { get; init; }
}
```

---

## Antelcat.DependencyInjectionEx.Autowired

Auto inject service from props/fields which marked with `AutowiredAttribute`

```csharp
IServiceCollection collection; // from somewhere

IServiceProvider provider = collection.BuildAutowiredServiceProviderEx();
```

---

## Antelcat.AspNetCore.DependencyInjectionEx.Autowired

Autowired service provider for ASP.NET

```csharp
var builder = WebApplication.CreateBuilder(args);

... multiple configures ...
                    
builder.Host.UseAutowiredServiceProviderFactory(); // should be configured before Build()

var app = builder.Build();
```

if you want to use `AutowiredAttribute` in `Controller` remember to

```csharp
builder.Services.AddControllers()
    .AddControllersAsServices(); // register controllers as services
```