// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text;
using Antelcat.DependencyInjectionEx.ServiceLookup;

namespace Antelcat.DependencyInjectionEx;

internal sealed class CallSiteJsonFormatter : CallSiteVisitor<CallSiteJsonFormatter.CallSiteFormatterContext, object?>
{
    internal static readonly CallSiteJsonFormatter Instance = new();

    private CallSiteJsonFormatter()
    {
    }

    public string Format(ServiceCallSite callSite)
    {
        var stringBuilder = new StringBuilder();
        var context       = new CallSiteFormatterContext(stringBuilder, 0, []);

        VisitCallSite(callSite, context);

        return stringBuilder.ToString();
    }

    protected override object? VisitConstructor(ConstructorCallSite constructorCallSite,
        CallSiteFormatterContext argument)
    {
        argument.WriteProperty("implementationType", constructorCallSite.ImplementationType);

        if (constructorCallSite.ParameterCallSites.Length <= 0) return null;
        argument.StartProperty("arguments");

        CallSiteFormatterContext childContext = argument.StartArray();
        foreach (ServiceCallSite parameter in constructorCallSite.ParameterCallSites)
        {
            childContext.StartArrayItem();
            VisitCallSite(parameter, childContext);
        }

        argument.EndArray();

        return null;
    }

    protected override object? VisitCallSiteMain(ServiceCallSite callSite, CallSiteFormatterContext argument)
    {
        if (argument.ShouldFormat(callSite))
        {
            CallSiteFormatterContext childContext = argument.StartObject();

            childContext.WriteProperty("serviceType", callSite.ServiceType);
            childContext.WriteProperty("kind", callSite.Kind);
            childContext.WriteProperty("cache", callSite.Cache.Location);

            base.VisitCallSiteMain(callSite, childContext);

            argument.EndObject();
        }
        else
        {
            CallSiteFormatterContext childContext = argument.StartObject();
            childContext.WriteProperty("ref", callSite.ServiceType);
            argument.EndObject();
        }

        return null;
    }

    protected override object? VisitConstant(ConstantCallSite constantCallSite, CallSiteFormatterContext argument)
    {
        argument.WriteProperty("value", constantCallSite.DefaultValue ?? "");

        return null;
    }

    protected override object? VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite,
        CallSiteFormatterContext argument)
    {
        return null;
    }

    protected override object? VisitIEnumerable(IEnumerableCallSite enumerableCallSite,
        CallSiteFormatterContext argument)
    {
        argument.WriteProperty("itemType", enumerableCallSite.ItemType);
        argument.WriteProperty("size", enumerableCallSite.ServiceCallSites.Length);

        if (enumerableCallSite.ServiceCallSites.Length <= 0) return null;
        argument.StartProperty("items");

        CallSiteFormatterContext childContext = argument.StartArray();
        foreach (ServiceCallSite item in enumerableCallSite.ServiceCallSites)
        {
            childContext.StartArrayItem();
            VisitCallSite(item, childContext);
        }

        argument.EndArray();
        return null;
    }

    protected override object? VisitFactory(FactoryCallSite factoryCallSite, CallSiteFormatterContext argument)
    {
        argument.WriteProperty("method", factoryCallSite.Factory.Method);

        return null;
    }

    internal struct CallSiteFormatterContext(
        StringBuilder builder,
        int offset,
        HashSet<ServiceCallSite> processedCallSites)
    {
        private bool _firstItem = true;

        public int           Offset  { get; } = offset;
        public StringBuilder Builder { get; } = builder;

        public bool ShouldFormat(ServiceCallSite serviceCallSite)
        {
            return processedCallSites.Add(serviceCallSite);
        }

        public CallSiteFormatterContext IncrementOffset()
        {
            return new CallSiteFormatterContext(Builder, Offset + 4, processedCallSites)
            {
                _firstItem = true
            };
        }

        public CallSiteFormatterContext StartObject()
        {
            Builder.Append('{');
            return IncrementOffset();
        }

        public void EndObject()
        {
            Builder.Append('}');
        }

        public void StartProperty(string name)
        {
            if (!_firstItem)
            {
                Builder.Append(',');
            }
            else
            {
                _firstItem = false;
            }

            Builder.Append('"').Append(name).Append("\":");
        }

        public void StartArrayItem()
        {
            if (!_firstItem)
            {
                Builder.Append(',');
            }
            else
            {
                _firstItem = false;
            }
        }

        public void WriteProperty(string name, object? value)
        {
            StartProperty(name);
            if (value != null)
            {
                Builder.Append(" \"").Append(value).Append('"');
            }
            else
            {
                Builder.Append("null");
            }
        }

        public CallSiteFormatterContext StartArray()
        {
            Builder.Append('[');
            return IncrementOffset();
        }

        public void EndArray()
        {
            Builder.Append(']');
        }
    }
}