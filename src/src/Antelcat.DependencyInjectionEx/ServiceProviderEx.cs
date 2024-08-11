// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Antelcat.DependencyInjectionEx.Callback;
using Antelcat.DependencyInjectionEx.ServiceLookup;
using Microsoft.Extensions.DependencyInjection;
using ResolveCallChain = Antelcat.DependencyInjectionEx.Callback.ResolveCallChain;

namespace Antelcat.DependencyInjectionEx;

/// <summary>
/// The default IServiceProvider.
/// </summary>
[DebuggerDisplay("{DebuggerToString(),nq}")]
[DebuggerTypeProxy(typeof(ServiceProviderDebugView))]
public sealed class ServiceProviderEx : IServiceProvider, IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly CallSiteValidator? callSiteValidator;

    private readonly CallbackTime callbackTime;

    // Internal for testing
    internal readonly ServiceProviderEngine Engine;

    private bool disposed;

    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> serviceAccessors;

    internal CallSiteFactory CallSiteFactory { get; }

    internal ServiceProviderEngineScope Root { get; }

    public event ServiceResolvedHandler? ServiceResolved;
    

    internal void OnServiceConstructed(IServiceProvider provider, Type serviceType, object instance, ServiceResolveKind kind) => 
        ServiceResolved?.Invoke(provider, serviceType, instance, kind);

    internal static bool VerifyOpenGenericServiceTrimmability { get; } =
        AppContext.TryGetSwitch("Antelcat.DependencyInjectionEx.VerifyOpenGenericServiceTrimmability", out bool verifyOpenGenerics) && verifyOpenGenerics;

    internal static bool DisableDynamicEngine { get; } =
        AppContext.TryGetSwitch("Antelcat.DependencyInjectionEx.DisableDynamicEngine", out bool disableDynamicEngine) && disableDynamicEngine;

    internal static bool VerifyAotCompatibility =>
#if NETFRAMEWORK || NETSTANDARD2_0
            false;
#else
        !RuntimeFeature.IsDynamicCodeSupported;
#endif

    internal ServiceProviderEx(ICollection<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options)
    {
        // note that Root needs to be set before calling GetEngine(), because the engine may need to access Root
        Root               = new ServiceProviderEngineScope(this, isRootScope: true);
        Engine             = GetEngine();
        serviceAccessors   = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
        var listenerKind   = options.ListenKind;
        CallSiteFactory = new CallSiteFactory(serviceDescriptors)
        {
            ReportSelector = ReportSelector
        };
        // The list of built-in services that aren't part of the list of service descriptors
        // keep this in sync with CallSiteFactory.IsService
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceProvider)), 
            new ServiceProviderCallSite(ReportSelector));
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceScopeFactory)), 
            new ConstantCallSite(ReportSelector,typeof(IServiceScopeFactory), Root));
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceProviderIsService)), 
            new ConstantCallSite(ReportSelector,typeof(IServiceProviderIsService), CallSiteFactory));
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceProviderIsKeyedService)),
            new ConstantCallSite(ReportSelector, typeof(IServiceProviderIsKeyedService), CallSiteFactory));

        callbackTime       = options.CallbackTime;

        if (options.ValidateScopes) callSiteValidator = new CallSiteValidator();

        if (options.ValidateOnBuild)
        {
            List<Exception>? exceptions = null;
            foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
            {
                try
                {
                    ValidateService(serviceDescriptor);
                }
                catch (Exception e)
                {
                    exceptions ??= [];
                    exceptions.Add(e);
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException("Some services are not able to be constructed", exceptions.ToArray());
            }
        }

        DependencyInjectionEventSource.Log.ServiceProviderBuilt(this);
        return;
        bool ReportSelector(CallSiteKind kind) => listenerKind.HasFlag(kind);
    }

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of the service to get.</param>
    /// <returns>The service that was produced.</returns>
    public object? GetService(Type serviceType) => GetService(ServiceIdentifier.FromServiceType(serviceType), Root);

    /// <summary>
    /// Gets the service object of the specified type with the specified key.
    /// </summary>
    /// <param name="serviceType">The type of the service to get.</param>
    /// <param name="serviceKey">The key of the service to get.</param>
    /// <returns>The keyed service.</returns>
    public object? GetKeyedService(Type serviceType, object? serviceKey)
        => GetKeyedService(serviceType, serviceKey, Root);

    internal object? GetKeyedService(Type serviceType, object? serviceKey, ServiceProviderEngineScope serviceProviderEngineScope)
        => GetService(new ServiceIdentifier(serviceKey, serviceType), serviceProviderEngineScope);

    /// <summary>
    /// Gets the service object of the specified type. Will throw if the service not found.
    /// </summary>
    /// <param name="serviceType">The type of the service to get.</param>
    /// <param name="serviceKey">The key of the service to get.</param>
    /// <returns>The keyed service.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        => GetRequiredKeyedService(serviceType, serviceKey, Root);

    internal object GetRequiredKeyedService(Type serviceType, object? serviceKey, ServiceProviderEngineScope serviceProviderEngineScope)
    {
        object? service = GetKeyedService(serviceType, serviceKey, serviceProviderEngineScope);
        return service ?? throw new InvalidOperationException(SR.Format(SR.NoServiceRegistered, serviceType));
    }

    internal bool IsDisposed() => disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeCore();
        Root.Dispose();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        DisposeCore();
        return Root.DisposeAsync();
    }

    private void DisposeCore()
    {
        disposed = true;
        DependencyInjectionEventSource.Log.ServiceProviderDisposed(this);
    }

    private void OnCreate(ServiceCallSite callSite)
    {
        callSiteValidator?.ValidateCallSite(callSite);
    }

    private void OnResolve(ServiceCallSite? callSite, IServiceScope scope)
    {
        if (callSite != null)
        {
            callSiteValidator?.ValidateResolution(callSite, scope, Root);
        }
    }

    private ResolveCallChain CreateCallChain(IServiceProvider provider) => callbackTime switch
    {
        CallbackTime.Immediately => new(new ImmediateResolveTrigger(OnServiceConstructed)
        {
            Provider = provider
        }),
        CallbackTime.Finally => new(new FinalResolveTrigger(OnServiceConstructed)
        {
            Provider = provider
        }),
        _ => throw new ArgumentException(nameof(callbackTime))
    };

    internal object? GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
    {
        if (disposed) ThrowHelper.ThrowObjectDisposedException();

        var serviceAccessor = serviceAccessors.GetOrAdd(serviceIdentifier,
            identifier => CreateServiceAccessor(identifier, CreateCallChain(serviceProviderEngineScope)));
        OnResolve(serviceAccessor.CallSite, serviceProviderEngineScope);
        DependencyInjectionEventSource.Log.ServiceResolved(this, serviceIdentifier.ServiceType);
        var chain = serviceAccessor.CallChain;
        chain.Provider = serviceProviderEngineScope;
        var wrap   = new ServiceProviderEngineScopeWrap(serviceProviderEngineScope, chain);
        var result = serviceAccessor.RealizedService?.Invoke(wrap);
        chain.OnResolved();
        Debug.Assert(result is null || CallSiteFactory.IsService(serviceIdentifier));
        return result;
    }

    private void ValidateService(ServiceDescriptor descriptor)
    {
        if (descriptor.ServiceType is { IsGenericType: true, IsConstructedGenericType: false }) return;

        try
        {
            ServiceCallSite? callSite = CallSiteFactory.GetCallSite(descriptor, new CallSiteChain());
            if (callSite != null)
            {
                OnCreate(callSite);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Error while validating the service descriptor '{descriptor}': {e.Message}", e);
        }
    }

    private ServiceAccessor CreateServiceAccessor(ServiceIdentifier serviceIdentifier, ResolveCallChain chain)
    {
        var callSite = CallSiteFactory.GetCallSite(serviceIdentifier, new CallSiteChain());
        if (callSite != null)
        {
            DependencyInjectionEventSource.Log.CallSiteBuilt(this, serviceIdentifier.ServiceType, callSite);
            OnCreate(callSite);

            // Optimize singleton case
            if (callSite.Cache.Location == CallSiteResultCacheLocation.Root)
            {
                object? value = CallSiteRuntimeResolver.Instance.Resolve(callSite,
                    new ServiceProviderEngineScopeWrap(Root, chain));
                return new ServiceAccessor
                {
                    CallSite        = callSite,
                    RealizedService = _ => value,
                    CallChain       = chain
                };
            }

            ServiceResolveHandler realizedService = Engine.RealizeService(callSite);
            return new ServiceAccessor { 
                CallSite = callSite, 
                RealizedService = realizedService ,
                CallChain = chain
            };
        }

        return new ServiceAccessor
        {
            CallSite        = callSite,
            RealizedService = _ => null,
            CallChain       = chain
        };
    }

    internal void ReplaceServiceAccessor(ServiceCallSite callSite, ServiceResolveHandler accessor, IServiceProvider provider)
    {
        serviceAccessors[new ServiceIdentifier(callSite.Key, callSite.ServiceType)] = new ServiceAccessor
        {
            CallSite        = callSite,
            RealizedService = accessor,
            CallChain       = CreateCallChain(provider)
        };
    }

    internal IServiceScope CreateScope()
    {
        if (disposed)
        {
            ThrowHelper.ThrowObjectDisposedException();
        }

        return new ServiceProviderEngineScope(this, isRootScope: false);
    }

    private ServiceProviderEngine GetEngine()
    {
        ServiceProviderEngine engine;

#if NETFRAMEWORK || NETSTANDARD2_0
            engine = CreateDynamicEngine();
#else
        if (RuntimeFeature.IsDynamicCodeCompiled && !DisableDynamicEngine)
        {
            engine = CreateDynamicEngine();
        }
        else
        {
            // Don't try to compile Expressions/IL if they are going to get interpreted
            engine = RuntimeServiceProviderEngine.Instance;
        }
#endif
        return engine;

        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "CreateDynamicEngine won't be called when using NativeAOT.")] // see also https://github.com/dotnet/linker/issues/2715
        ServiceProviderEngine CreateDynamicEngine() => new DynamicServiceProviderEngine(this);
    }

    private string DebuggerToString() => Root.DebuggerToString();

    internal sealed class ServiceProviderDebugView(ServiceProviderEx serviceProvider)
    {
        public List<ServiceDescriptor> ServiceDescriptors =>
            [..serviceProvider.Root.RootProviderEx.CallSiteFactory.Descriptors];
        public List<object> Disposables => [..serviceProvider.Root.Disposables];
        public bool         Disposed    => serviceProvider.Root.Disposed;
        public bool         IsScope     => !serviceProvider.Root.IsRootScope;
    }

    private sealed class ServiceAccessor
    {
        public ServiceCallSite?       CallSite        { get; set; }
        public ServiceResolveHandler? RealizedService { get; set; }
        
        public required ResolveCallChain CallChain { get; set; }
    }
}