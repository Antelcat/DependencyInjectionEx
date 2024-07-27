// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Antelcat.DependencyInjectionEx.ServiceLookup
{
    internal sealed class CallSiteFactory : IServiceProviderIsService, IServiceProviderIsKeyedService
    {
        private const    int                                                       DefaultSlot = 0;
        private readonly ServiceDescriptor[]                                       descriptors;
        private readonly ConcurrentDictionary<ServiceCacheKey, ServiceCallSite>    callSiteCache    = new();
        private readonly Dictionary<ServiceIdentifier, ServiceDescriptorCacheItem> descriptorLookup = new();
        private readonly ConcurrentDictionary<ServiceIdentifier, object>           callSiteLocks    = new();

        private readonly StackGuard stackGuard;

        private readonly ServiceResolvedHandler serviceResolved;
        
        public CallSiteFactory(ICollection<ServiceDescriptor> descriptors, ServiceResolvedHandler serviceResolved)
        {
            this.serviceResolved = serviceResolved;
            stackGuard           = new StackGuard();
            this.descriptors     = new ServiceDescriptor[descriptors.Count];
            descriptors.CopyTo(this.descriptors, 0);

            Populate();
        }

        internal ServiceDescriptor[] Descriptors => descriptors;

        private void Populate()
        {
            foreach (ServiceDescriptor descriptor in descriptors)
            {
                Type serviceType = descriptor.ServiceType;
                if (serviceType.IsGenericTypeDefinition)
                {
                    Type? implementationType = descriptor.GetImplementationType();

                    if (implementationType == null || !implementationType.IsGenericTypeDefinition)
                    {
                        throw new ArgumentException(
                            SR.Format(SR.OpenGenericServiceRequiresOpenGenericImplementation, serviceType),
                            nameof(descriptors));
                    }

                    if (implementationType.IsAbstract || implementationType.IsInterface)
                    {
                        throw new ArgumentException(
                            SR.Format(SR.TypeCannotBeActivated, implementationType, serviceType));
                    }

                    Type[] serviceTypeGenericArguments = serviceType.GetGenericArguments();
                    Type[] implementationTypeGenericArguments = implementationType.GetGenericArguments();
                    if (serviceTypeGenericArguments.Length != implementationTypeGenericArguments.Length)
                    {
                        throw new ArgumentException(
                            SR.Format(SR.ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation, serviceType, implementationType), "descriptors");
                    }

                    if (ServiceProvider.VerifyOpenGenericServiceTrimmability)
                    {
                        ValidateTrimmingAnnotations(serviceType, serviceTypeGenericArguments, implementationType, implementationTypeGenericArguments);
                    }
                }
                else if (descriptor.TryGetImplementationType(out Type? implementationType))
                {
                    Debug.Assert(implementationType != null);

                    if (implementationType.IsGenericTypeDefinition ||
                        implementationType.IsAbstract ||
                        implementationType.IsInterface)
                    {
                        throw new ArgumentException(
                            SR.Format(SR.TypeCannotBeActivated, implementationType, serviceType));
                    }
                }

                var cacheKey = ServiceIdentifier.FromDescriptor(descriptor);
                descriptorLookup.TryGetValue(cacheKey, out ServiceDescriptorCacheItem cacheItem);
                descriptorLookup[cacheKey] = cacheItem.Add(descriptor);
            }
        }

        /// <summary>
        /// Validates that two generic type definitions have compatible trimming annotations on their generic arguments.
        /// </summary>
        /// <remarks>
        /// When open generic types are used in DI, there is an error when the concrete implementation type
        /// has [DynamicallyAccessedMembers] attributes on a generic argument type, but the interface/service type
        /// doesn't have matching annotations. The problem is that the trimmer doesn't see the members that need to
        /// be preserved on the type being passed to the generic argument. But when the interface/service type also has
        /// the annotations, the trimmer will see which members need to be preserved on the closed generic argument type.
        /// </remarks>
        private static void ValidateTrimmingAnnotations(
            Type serviceType,
            Type[] serviceTypeGenericArguments,
            Type implementationType,
            Type[] implementationTypeGenericArguments)
        {
            Debug.Assert(serviceTypeGenericArguments.Length == implementationTypeGenericArguments.Length);

            for (int i = 0; i < serviceTypeGenericArguments.Length; i++)
            {
                Type serviceGenericType = serviceTypeGenericArguments[i];
                Type implementationGenericType = implementationTypeGenericArguments[i];

                DynamicallyAccessedMemberTypes serviceDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(serviceGenericType);
                DynamicallyAccessedMemberTypes implementationDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(implementationGenericType);

                if (!AreCompatible(serviceDynamicallyAccessedMembers, implementationDynamicallyAccessedMembers))
                {
                    throw new ArgumentException(SR.Format(SR.TrimmingAnnotationsDoNotMatch, implementationType.FullName, serviceType.FullName));
                }

                bool serviceHasNewConstraint = serviceGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
                bool implementationHasNewConstraint = implementationGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
                if (implementationHasNewConstraint && !serviceHasNewConstraint)
                {
                    throw new ArgumentException(SR.Format(SR.TrimmingAnnotationsDoNotMatch_NewConstraint, implementationType.FullName, serviceType.FullName));
                }
            }
        }

        private static DynamicallyAccessedMemberTypes GetDynamicallyAccessedMemberTypes(Type serviceGenericType)
        {
            foreach (CustomAttributeData attributeData in serviceGenericType.GetCustomAttributesData())
            {
                if (attributeData.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute" &&
                    attributeData.ConstructorArguments is [{ ArgumentType.FullName: "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes" } _])
                {
                    return (DynamicallyAccessedMemberTypes)(int)attributeData.ConstructorArguments[0].Value!;
                }
            }

            return DynamicallyAccessedMemberTypes.None;
        }

        private static bool AreCompatible(DynamicallyAccessedMemberTypes serviceDynamicallyAccessedMembers, DynamicallyAccessedMemberTypes implementationDynamicallyAccessedMembers)
        {
            // The DynamicallyAccessedMemberTypes don't need to exactly match.
            // The service type needs to preserve a superset of the members required by the implementation type.
            return serviceDynamicallyAccessedMembers.HasFlag(implementationDynamicallyAccessedMembers);
        }

        // For unit testing
        internal int? GetSlot(ServiceDescriptor serviceDescriptor)
        {
            if (descriptorLookup.TryGetValue(ServiceIdentifier.FromDescriptor(serviceDescriptor), out ServiceDescriptorCacheItem item))
            {
                return item.GetSlot(serviceDescriptor);
            }

            return null;
        }

        internal ServiceCallSite? GetCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain) =>
            callSiteCache.TryGetValue(new ServiceCacheKey(serviceIdentifier, DefaultSlot), out ServiceCallSite? site) ? site :
            CreateCallSite(serviceIdentifier, callSiteChain);

        internal ServiceCallSite? GetCallSite(ServiceDescriptor serviceDescriptor, CallSiteChain callSiteChain)
        {
            var serviceIdentifier = ServiceIdentifier.FromDescriptor(serviceDescriptor);
            if (descriptorLookup.TryGetValue(serviceIdentifier, out ServiceDescriptorCacheItem descriptor))
            {
                return TryCreateExact(serviceDescriptor, serviceIdentifier, callSiteChain, descriptor.GetSlot(serviceDescriptor));
            }

            Debug.Fail("_descriptorLookup didn't contain requested serviceDescriptor");
            return null;
        }

        private ServiceCallSite? CreateCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
        {
            if (!stackGuard.TryEnterOnCurrentStack())
            {
                return stackGuard.RunOnEmptyStack(CreateCallSite, serviceIdentifier, callSiteChain);
            }

            // We need to lock the resolution process for a single service type at a time:
            // Consider the following:
            // C -> D -> A
            // E -> D -> A
            // Resolving C and E in parallel means that they will be modifying the callsite cache concurrently
            // to add the entry for C and E, but the resolution of D and A is synchronized
            // to make sure C and E both reference the same instance of the callsite.

            // This is to make sure we can safely store singleton values on the callsites themselves

            var callsiteLock = callSiteLocks.GetOrAdd(serviceIdentifier, static _ => new object());

            lock (callsiteLock)
            {
                callSiteChain.CheckCircularDependency(serviceIdentifier);

                ServiceCallSite? callSite = TryCreateExact(serviceIdentifier, callSiteChain) ??
                                           TryCreateOpenGeneric(serviceIdentifier, callSiteChain) ??
                                           TryCreateEnumerable(serviceIdentifier, callSiteChain);

                return callSite;
            }
        }

        private ServiceCallSite? TryCreateExact(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
        {
            if (descriptorLookup.TryGetValue(serviceIdentifier, out ServiceDescriptorCacheItem descriptor))
            {
                return TryCreateExact(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot);
            }

            if (serviceIdentifier.ServiceKey == null) return null;
            // Check if there is a registration with KeyedService.AnyKey
            var catchAllIdentifier = new ServiceIdentifier(KeyedService.AnyKey, serviceIdentifier.ServiceType);
            return descriptorLookup.TryGetValue(catchAllIdentifier, out descriptor)
                ? TryCreateExact(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot)
                : null;
        }

        private ServiceCallSite? TryCreateOpenGeneric(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
        {
            if (!serviceIdentifier.IsConstructedGenericType) return null;
            var genericIdentifier = serviceIdentifier.GetGenericTypeDefinition();
            if (descriptorLookup.TryGetValue(genericIdentifier, out ServiceDescriptorCacheItem descriptor))
            {
                return TryCreateOpenGeneric(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot, true);
            }

            if (serviceIdentifier.ServiceKey == null) return null;
            // Check if there is a registration with KeyedService.AnyKey
            var catchAllIdentifier = new ServiceIdentifier(KeyedService.AnyKey, genericIdentifier.ServiceType);
            return descriptorLookup.TryGetValue(catchAllIdentifier, out descriptor)
                ? TryCreateOpenGeneric(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot, true)
                : null;
        }

        private ServiceCallSite? TryCreateEnumerable(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
        {
            ServiceCacheKey callSiteKey = new ServiceCacheKey(serviceIdentifier, DefaultSlot);
            if (callSiteCache.TryGetValue(callSiteKey, out ServiceCallSite? serviceCallSite))
            {
                return serviceCallSite;
            }

            try
            {
                callSiteChain.Add(serviceIdentifier);

                var serviceType = serviceIdentifier.ServiceType;

                if (!serviceType.IsConstructedGenericType ||
                    serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                {
                    return null;
                }

                Type itemType = serviceType.GenericTypeArguments[0];
                var cacheKey = new ServiceIdentifier(serviceIdentifier.ServiceKey, itemType);
                if (ServiceProvider.VerifyAotCompatibility && itemType.IsValueType)
                {
                    // NativeAOT apps are not able to make Enumerable of ValueType services
                    // since there is no guarantee the ValueType[] code has been generated.
                    throw new InvalidOperationException(SR.Format(SR.AotCannotCreateEnumerableValueType, itemType));
                }

                CallSiteResultCacheLocation cacheLocation = CallSiteResultCacheLocation.Root;
                ServiceCallSite[] callSites;

                // If item type is not generic we can safely use descriptor cache
                if (!itemType.IsConstructedGenericType &&
                    descriptorLookup.TryGetValue(cacheKey, out ServiceDescriptorCacheItem descriptors))
                {
                    callSites = new ServiceCallSite[descriptors.Count];
                    for (int i = 0; i < descriptors.Count; i++)
                    {
                        ServiceDescriptor descriptor = descriptors[i];

                        // Last service should get slot 0
                        int slot = descriptors.Count - i - 1;
                        // There may not be any open generics here
                        ServiceCallSite? callSite = TryCreateExact(descriptor, cacheKey, callSiteChain, slot);
                        Debug.Assert(callSite != null);

                        cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                        callSites[i] = callSite;
                    }
                }
                else
                {
                    // We need to construct a list of matching call sites in declaration order, but to ensure
                    // correct caching we must assign slots in reverse declaration order and with slots being
                    // given out first to any exact matches before any open generic matches. Therefore, we
                    // iterate over the descriptors twice in reverse, catching exact matches on the first pass
                    // and open generic matches on the second pass.

                    List<KeyValuePair<int, ServiceCallSite>> callSitesByIndex = new();

                    int slot = 0;
                    for (int i = this.descriptors.Length - 1; i >= 0; i--)
                    {
                        if (!KeysMatch(this.descriptors[i].ServiceKey, cacheKey.ServiceKey)) continue;
                        if (TryCreateExact(this.descriptors[i], cacheKey, callSiteChain, slot) is not { } callSite) continue;
                        AddCallSite(callSite, i);
                    }
                    for (int i = this.descriptors.Length - 1; i >= 0; i--)
                    {
                        if (!KeysMatch(this.descriptors[i].ServiceKey, cacheKey.ServiceKey)) continue;
                        if (TryCreateOpenGeneric(this.descriptors[i], cacheKey, callSiteChain, slot,
                                throwOnConstraintViolation: false) is not { } callSite) continue;
                        AddCallSite(callSite, i);
                    }

                    callSitesByIndex.Sort((a, b) => a.Key.CompareTo(b.Key));
                    callSites = new ServiceCallSite[callSitesByIndex.Count];
                    for (var i = 0; i < callSites.Length; ++i)
                    {
                        callSites[i] = callSitesByIndex[i].Value;
                    }

                    void AddCallSite(ServiceCallSite callSite, int index)
                    {
                        slot++;

                        cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                        callSitesByIndex.Add(new(index, callSite));
                    }
                }
                ResultCache resultCache = (cacheLocation == CallSiteResultCacheLocation.Scope || cacheLocation == CallSiteResultCacheLocation.Root)
                    ? new ResultCache(cacheLocation, callSiteKey)
                    : new ResultCache(CallSiteResultCacheLocation.None, callSiteKey);
                return callSiteCache[callSiteKey] = new IEnumerableCallSite(resultCache, itemType, callSites)
                    { OnResolve = serviceResolved };
            }
            finally
            {
                callSiteChain.Remove(serviceIdentifier);
            }
        }

        private static CallSiteResultCacheLocation GetCommonCacheLocation(CallSiteResultCacheLocation locationA, CallSiteResultCacheLocation locationB)
        {
            return (CallSiteResultCacheLocation)Math.Max((int)locationA, (int)locationB);
        }

        private ServiceCallSite? TryCreateExact(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain, int slot)
        {
            if (serviceIdentifier.ServiceType != descriptor.ServiceType) return null;
            ServiceCacheKey callSiteKey = new ServiceCacheKey(serviceIdentifier, slot);
            if (callSiteCache.TryGetValue(callSiteKey, out ServiceCallSite? serviceCallSite))
            {
                return serviceCallSite;
            }

            ServiceCallSite callSite;
            var             lifetime = new ResultCache(descriptor.Lifetime, serviceIdentifier, slot);
            if (descriptor.HasImplementationInstance())
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.GetImplementationInstance()){
                    OnResolve = serviceResolved
                };
            }
            else switch (descriptor)
            {
                case { IsKeyedService: false, ImplementationFactory: not null }:
                    callSite = new FactoryCallSite(lifetime,
                        descriptor.ServiceType,
                        descriptor.ImplementationFactory)
                    {
                        OnResolve = serviceResolved
                    };
                    break;
                case { IsKeyedService: true, KeyedImplementationFactory: not null }:
                    callSite = new FactoryCallSite(lifetime, 
                        descriptor.ServiceType, 
                        serviceIdentifier.ServiceKey!,
                        descriptor.KeyedImplementationFactory)
                    {
                        OnResolve = serviceResolved
                    };
                    break;
                default:
                {
                    if (descriptor.HasImplementationType())
                    {
                        callSite = CreateConstructorCallSite(lifetime, serviceIdentifier, descriptor.GetImplementationType()!, callSiteChain);
                    }
                    else
                    {
                        throw new InvalidOperationException(SR.InvalidServiceDescriptor);
                    }

                    break;
                }
            }
            callSite.Key = descriptor.ServiceKey;

            return callSiteCache[callSiteKey] = callSite;

        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055:MakeGenericType",
            Justification = "MakeGenericType here is used to create a closed generic implementation type given the closed service type. " +
            "Trimming annotations on the generic types are verified when 'Antelcat.DependencyInjectionEx.VerifyOpenGenericServiceTrimmability' is set, which is set by default when PublishTrimmed=true. " +
            "That check informs developers when these generic types don't have compatible trimming annotations.")]
        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "When ServiceProvider.VerifyAotCompatibility is true, which it is by default when PublishAot=true, " +
            "this method ensures the generic types being created aren't using ValueTypes.")]
        private ServiceCallSite? TryCreateOpenGeneric(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain, int slot, bool throwOnConstraintViolation)
        {
            if (serviceIdentifier.IsConstructedGenericType &&
                serviceIdentifier.ServiceType.GetGenericTypeDefinition() == descriptor.ServiceType)
            {
                ServiceCacheKey callSiteKey = new ServiceCacheKey(serviceIdentifier, slot);
                if (callSiteCache.TryGetValue(callSiteKey, out ServiceCallSite? serviceCallSite))
                {
                    return serviceCallSite;
                }

                Type? implementationType = descriptor.GetImplementationType();
                Debug.Assert(implementationType != null, "descriptor.ImplementationType != null");
                var lifetime = new ResultCache(descriptor.Lifetime, serviceIdentifier, slot);
                Type closedType;
                try
                {
                    Type[] genericTypeArguments = serviceIdentifier.ServiceType.GenericTypeArguments;
                    if (ServiceProvider.VerifyAotCompatibility)
                    {
                        VerifyOpenGenericAotCompatibility(serviceIdentifier.ServiceType, genericTypeArguments);
                    }

                    closedType = implementationType.MakeGenericType(genericTypeArguments);
                }
                catch (ArgumentException)
                {
                    if (throwOnConstraintViolation)
                    {
                        throw;
                    }

                    return null;
                }

                return callSiteCache[callSiteKey] = CreateConstructorCallSite(lifetime, serviceIdentifier, closedType, callSiteChain);
            }

            return null;
        }

        private ConstructorCallSite CreateConstructorCallSite(
            ResultCache lifetime,
            ServiceIdentifier serviceIdentifier,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
            CallSiteChain callSiteChain)
        {
            try
            {
                callSiteChain.Add(serviceIdentifier, implementationType);
                ConstructorInfo[] constructors = implementationType.GetConstructors();

                ServiceCallSite[]? parameterCallSites = null;

                switch (constructors.Length)
                {
                    case 0:
                        throw new InvalidOperationException(SR.Format(SR.NoConstructorMatch, implementationType));
                    case 1:
                    {
                        ConstructorInfo constructor = constructors[0];
                        ParameterInfo[] parameters  = constructor.GetParameters();
                        if (parameters.Length == 0)
                        {
                            return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor)
                            {
                                OnResolve = serviceResolved
                            };
                        }

                        parameterCallSites = CreateArgumentCallSites(
                            serviceIdentifier,
                            implementationType,
                            callSiteChain,
                            parameters,
                            throwIfCallSiteNotFound: true)!;

                        return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor,
                            parameterCallSites)
                        {
                            OnResolve = serviceResolved
                        };
                    }
                }

                Array.Sort(constructors,
                    (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

                ConstructorInfo? bestConstructor = null;
                HashSet<Type>? bestConstructorParameterTypes = null;
                foreach (var constructor in constructors)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();

                    ServiceCallSite[]? currentParameterCallSites = CreateArgumentCallSites(
                        serviceIdentifier,
                        implementationType,
                        callSiteChain,
                        parameters,
                        throwIfCallSiteNotFound: false);

                    if (currentParameterCallSites == null) continue;
                    if (bestConstructor == null)
                    {
                        bestConstructor    = constructor;
                        parameterCallSites = currentParameterCallSites;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>();
                            foreach (ParameterInfo p in bestConstructor.GetParameters())
                            {
                                bestConstructorParameterTypes.Add(p.ParameterType);
                            }
                        }

                        foreach (ParameterInfo p in parameters)
                        {
                            if (!bestConstructorParameterTypes.Contains(p.ParameterType))
                            {
                                // Ambiguous match exception
                                throw new InvalidOperationException(string.Join(
                                    Environment.NewLine,
                                    SR.Format(SR.AmbiguousConstructorException, implementationType),
                                    bestConstructor,
                                    constructor));
                            }
                        }
                    }
                }

                if (bestConstructor == null)
                {
                    throw new InvalidOperationException(
                        SR.Format(SR.UnableToActivateTypeException, implementationType));
                }
                else
                {
                    Debug.Assert(parameterCallSites != null);
                    return new ConstructorCallSite(lifetime,
                        serviceIdentifier.ServiceType,
                        bestConstructor,
                        parameterCallSites)
                    {
                        OnResolve = serviceResolved
                    };
                }
            }
            finally
            {
                callSiteChain.Remove(serviceIdentifier);
            }
        }

        /// <returns>Not <b>null</b> if <b>throwIfCallSiteNotFound</b> is true</returns>
        private ServiceCallSite[]? CreateArgumentCallSites(
            ServiceIdentifier serviceIdentifier,
            Type implementationType,
            CallSiteChain callSiteChain,
            ParameterInfo[] parameters,
            bool throwIfCallSiteNotFound)
        {
            var parameterCallSites = new ServiceCallSite[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                ServiceCallSite? callSite = null;
                Type parameterType = parameters[index].ParameterType;
                foreach (var attribute in parameters[index].GetCustomAttributes(true))
                {
                    if (serviceIdentifier.ServiceKey != null && attribute is ServiceKeyAttribute)
                    {
                        // Check if the parameter type matches
                        if (parameterType != serviceIdentifier.ServiceKey.GetType())
                        {
                            throw new InvalidOperationException(SR.InvalidServiceKeyType);
                        }

                        callSite = new ConstantCallSite(parameterType, serviceIdentifier.ServiceKey)
                        {
                            OnResolve = serviceResolved
                        };
                        break;
                    }

                    if (attribute is not FromKeyedServicesAttribute keyed) continue;
                    var parameterSvcId = new ServiceIdentifier(keyed.Key, parameterType);
                    callSite = GetCallSite(parameterSvcId, callSiteChain);
                    break;
                }

                callSite ??= GetCallSite(ServiceIdentifier.FromServiceType(parameterType), callSiteChain);

                if (callSite == null && ParameterDefaultValue.TryGetDefaultValue(parameters[index], out object? defaultValue))
                {
                    callSite = new ConstantCallSite(parameterType, defaultValue)
                    {
                        OnResolve = serviceResolved
                    };
                }

                if (callSite == null)
                {
                    if (throwIfCallSiteNotFound)
                    {
                        throw new InvalidOperationException(SR.Format(SR.CannotResolveService,
                            parameterType,
                            implementationType));
                    }

                    return null;
                }

                parameterCallSites[index] = callSite;
            }

            return parameterCallSites;
        }

        /// <summary>
        /// Verifies none of the generic type arguments are ValueTypes.
        /// </summary>
        /// <remarks>
        /// NativeAOT apps are not guaranteed that the native code for the closed generic of ValueType
        /// has been generated. To catch these problems early, this verification is enabled at development-time
        /// to inform the developer early that this scenario will not work once AOT'd.
        /// </remarks>
        private static void VerifyOpenGenericAotCompatibility(Type serviceType, Type[] genericTypeArguments)
        {
            foreach (Type typeArg in genericTypeArguments)
            {
                if (typeArg.IsValueType)
                {
                    throw new InvalidOperationException(SR.Format(SR.AotCannotCreateGenericValueType, serviceType, typeArg));
                }
            }
        }

        public void Add(ServiceIdentifier serviceIdentifier, ServiceCallSite serviceCallSite)
        {
            callSiteCache[new ServiceCacheKey(serviceIdentifier, DefaultSlot)] = serviceCallSite;
        }

        public bool IsService(Type serviceType) => IsService(new ServiceIdentifier(null, serviceType));

        public bool IsKeyedService(Type serviceType, object? key) => IsService(new ServiceIdentifier(key, serviceType));

        internal bool IsService(ServiceIdentifier serviceIdentifier)
        {
            var serviceType = serviceIdentifier.ServiceType;

            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            // Querying for an open generic should return false (they aren't resolvable)
            if (serviceType.IsGenericTypeDefinition)
            {
                return false;
            }

            if (descriptorLookup.ContainsKey(serviceIdentifier))
            {
                return true;
            }

            if (serviceIdentifier.ServiceKey != null && descriptorLookup.ContainsKey(new ServiceIdentifier(KeyedService.AnyKey, serviceType)))
            {
                return true;
            }

            if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() is { } genericDefinition)
            {
                // We special case IEnumerable since it isn't explicitly registered in the container
                // yet we can manifest instances of it when requested.
                return genericDefinition == typeof(IEnumerable<>) || descriptorLookup.ContainsKey(serviceIdentifier.GetGenericTypeDefinition());
            }

            // These are the built in service types that aren't part of the list of service descriptors
            // If you update these make sure to also update the code in ServiceProvider.ctor
            return serviceType == typeof(IServiceProvider) ||
                   serviceType == typeof(IServiceScopeFactory) ||
                   serviceType == typeof(IServiceProviderIsService) ||
                   serviceType == typeof(IServiceProviderIsKeyedService);
        }

        /// <summary>
        /// Returns true if both keys are null or equals, or if key1 is KeyedService.AnyKey and key2 is not null
        /// </summary>
        private static bool KeysMatch(object? key1, object? key2)
        {
            if (key1 == null && key2 == null)
                return true;

            if (key1 != null && key2 != null)
                return key1.Equals(KeyedService.AnyKey) || key1.Equals(key2);

            return false;
        }

        private struct ServiceDescriptorCacheItem
        {
            [DisallowNull]
            private ServiceDescriptor? _item;

            [DisallowNull]
            private List<ServiceDescriptor>? _items;

            public ServiceDescriptor Last
            {
                get
                {
                    if (_items is { Count: > 0 }) return _items[^1];

                    Debug.Assert(_item != null);
                    return _item;
                }
            }

            public int Count
            {
                get
                {
                    if (_item == null)
                    {
                        Debug.Assert(_items == null);
                        return 0;
                    }

                    return 1 + (_items?.Count ?? 0);
                }
            }

            public ServiceDescriptor this[int index]
            {
                get
                {
                    if (index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

                    return index == 0 ? _item! : _items![index - 1];
                }
            }

            public int GetSlot(ServiceDescriptor descriptor)
            {
                if (descriptor == _item)
                {
                    return Count - 1;
                }

                if (_items == null) throw new InvalidOperationException(SR.ServiceDescriptorNotExist);
                int index = _items.IndexOf(descriptor);
                if (index != -1) return _items.Count - (index + 1);

                throw new InvalidOperationException(SR.ServiceDescriptorNotExist);
            }

            public ServiceDescriptorCacheItem Add(ServiceDescriptor descriptor)
            {
                var newCacheItem = default(ServiceDescriptorCacheItem);
                if (_item == null)
                {
                    Debug.Assert(_items == null);
                    newCacheItem._item = descriptor;
                }
                else
                {
                    newCacheItem._item = _item;
                    newCacheItem._items = _items ?? new List<ServiceDescriptor>();
                    newCacheItem._items.Add(descriptor);
                }
                return newCacheItem;
            }
        }
    }
}
