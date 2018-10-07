using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnPreBindingAttribute : BaseModelBindAttribute
    {
        static ConcurrentDictionary<Type, List<MethodInfo>> CustomBindMethods = new ConcurrentDictionary<Type, List<MethodInfo>>();

        internal static void Execute(ControllerContext cContext, object model)
        {
            var methods = DiscoverBindMethods(cContext, model, typeof(OnPreBindingAttribute), CustomBindMethods);

            foreach (var action in methods) action();
        }
    }
}