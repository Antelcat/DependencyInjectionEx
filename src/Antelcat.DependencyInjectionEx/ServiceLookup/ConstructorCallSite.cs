// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ConstructorCallSite : ServiceCallSite
{
    internal ConstructorInfo   ConstructorInfo    { get; }
    internal ServiceCallSite[] ParameterCallSites { get; }

    public ConstructorCallSite(ResultCache cache,
        Func<CallSiteKind, bool> reportSelector,
        Type serviceType,
        ConstructorInfo constructorInfo) : this(cache, reportSelector, serviceType, constructorInfo, [])
    {
    }

    public ConstructorCallSite(ResultCache cache, 
        Func<CallSiteKind, bool> reportSelector, 
        Type serviceType,
        ConstructorInfo constructorInfo,
        ServiceCallSite[] parameterCallSites) : base(cache, reportSelector)
    {
        if (!serviceType.IsAssignableFrom(constructorInfo.DeclaringType))
        {
            throw new ArgumentException(SR.Format(SR.ImplementationTypeCantBeConvertedToServiceType,
                constructorInfo.DeclaringType, serviceType));
        }

        ServiceType        = serviceType;
        ConstructorInfo    = constructorInfo;
        ParameterCallSites = parameterCallSites;
    }

    public override Type ServiceType { get; }

    public override Type?        ImplementationType => ConstructorInfo.DeclaringType;
    public override CallSiteKind Kind               { get; } = CallSiteKind.Constructor;
}