global using ServiceResolveHandler = System.Func<
    Antelcat.DependencyInjectionEx.ServiceLookup.ServiceProviderEngineScopeWrap, 
    //Antelcat.DependencyInjectionEx.ServiceLookup.ResolveCallChain,
    object?>;
using Antelcat.DependencyInjectionEx.ServiceLookup;


namespace Antelcat.DependencyInjectionEx;

//internal delegate object? ServiceResolveHandler(ServiceProviderEngineScope scope/*, ResolveCallChain callChain*/);
