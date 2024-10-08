// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Antelcat.DependencyInjectionEx.ServiceLookup;

internal enum CallSiteResultCacheLocation
{
    Root,
    Scope,
    Dispose,
    None
}