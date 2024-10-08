// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

[method: RequiresDynamicCode("Creates DynamicMethods")]
internal sealed class DynamicServiceProviderEngine(ServiceProviderEx serviceProvider)
    : CompiledServiceProviderEngine(serviceProvider)
{
    private readonly ServiceProviderEx serviceProvider = serviceProvider;

    public override ServiceResolveHandler RealizeService(ServiceCallSite callSite)
    {
        int callCount = 0;
        return scope =>
        {
            // Resolve the result before we increment the call count, this ensures that singletons
            // won't cause any side effects during the compilation of the resolve function.
            var result = CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);

            if (Interlocked.Increment(ref callCount) == 2)
            {
                // Don't capture the ExecutionContext when forking to build the compiled version of the
                // resolve function
                _ = ThreadPool.UnsafeQueueUserWorkItem(_ =>
                    {
                        try
                        {
                            serviceProvider.ReplaceServiceAccessor(callSite, base.RealizeService(callSite), scope);
                        }
                        catch (Exception ex)
                        {
                            DependencyInjectionEventSource.Log.ServiceRealizationFailed(ex, serviceProvider.GetHashCode());

                            Debug.Fail($"We should never get exceptions from the background compilation.{Environment.NewLine}{ex}");
                        }
                    },
                    null);
            }

            return result;
        };
    }
}