# Antelcat.DependencyInjectionEx

Rebuild from [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

Export service resolve callback

> Service resolve callback will be triggered after a resolve request is completed

## Antelcat.DependencyInjectionEx.Autowired

Auto inject service from props/fields which marked with `AutowiredAttribute`

> Autowired resolve will only be activated when the service is resolved by "Constructor"