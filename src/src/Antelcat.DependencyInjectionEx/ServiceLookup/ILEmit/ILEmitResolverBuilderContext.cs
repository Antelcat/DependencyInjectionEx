// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ILEmitResolverBuilderContext(ILGenerator generator)
{
    public ILGenerator                           Generator { get; } = generator;
    public List<object?>?                        Constants { get; set; }
    public List<Func<IServiceProvider, object>>? Factories { get; set; }
}