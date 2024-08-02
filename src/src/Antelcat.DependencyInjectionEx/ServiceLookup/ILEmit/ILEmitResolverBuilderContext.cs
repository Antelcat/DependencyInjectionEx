// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Antelcat.DependencyInjectionEx.Callback;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal sealed class ILEmitResolverBuilderContext(ILGenerator generator)
{
    public  ILGenerator                           Generator  { get; } = generator;
    public  List<object?>?                        Constants  { get; set; }
    public  List<Func<IServiceProvider, object>>? Factories  { get; set; }
    public  List<ServiceCallSite>?                CallSites  { get; set; }

    public LocalBuilder LocalChain
    {
        get
        {
            if (localChain is not null) return localChain;
            localChain = Generator.DeclareLocal(typeof(ResolveCallChain));
            Generator.Emit(OpCodes.Ldarg_1);
            Generator.Emit(OpCodes.Callvirt, ILEmitResolverBuilder.CallChain);
            Generator.Emit(OpCodes.Stloc, localChain);
            return localChain;
        }
    }

    private LocalBuilder? localChain;
}