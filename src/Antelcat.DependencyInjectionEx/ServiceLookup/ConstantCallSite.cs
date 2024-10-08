// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ConstantCallSite : ServiceCallSite
{
    private readonly Type    serviceType;
    internal         object? DefaultValue => Value;

    public ConstantCallSite(Func<CallSiteKind, bool> reportSelector, Type serviceType, object? defaultValue)
        : base(ResultCache.None(serviceType), reportSelector)
    {
        this.serviceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        if (defaultValue != null && !serviceType.IsInstanceOfType(defaultValue))
        {
            throw new ArgumentException(SR.Format(SR.ConstantCantBeConvertedToServiceType, defaultValue.GetType(),
                serviceType));
        }

        Value = defaultValue;
    }

    public override Type         ServiceType        => serviceType;
    public override Type         ImplementationType => DefaultValue?.GetType() ?? serviceType;
    public override CallSiteKind Kind               { get; } = CallSiteKind.Constant;
}