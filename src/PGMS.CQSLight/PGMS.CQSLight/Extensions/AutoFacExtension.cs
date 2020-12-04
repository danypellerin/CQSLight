using Autofac;
using System;
using System.Collections.Generic;

namespace PGMS.CQSLight.Extensions
{
    public static class AutoFacComponentContextExtension
    {
        public static IEnumerable<object> ResolveAll(this IComponentContext context, Type type)
        {
            var listGenericType = typeof(IEnumerable<>).MakeGenericType(new[] { type });
            var result = context.Resolve(listGenericType);
            return (IEnumerable<object>)result;
        }
    }
}