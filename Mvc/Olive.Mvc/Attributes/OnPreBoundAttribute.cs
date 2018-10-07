using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnPreBoundAttribute : BaseModelBindAttribute
    {
        static ConcurrentDictionary<Type, List<MethodInfo>> CustomBindMethods = new ConcurrentDictionary<Type, List<MethodInfo>>();

        const string ROOT = "_Olive.CustomPreBindActions.RootBinder";
        const string ACTIONS = "_Olive.CustomPreBindActions";

        internal static void Enqueue(ControllerContext cContext, object model) =>
            Enqueue(cContext, model, ACTIONS, typeof(OnPreBoundAttribute), CustomBindMethods);

        internal static void SetRoot(ModelBindingContext model) => SetRoot(model, ROOT);

        internal static void InvokeAllForRoot(ModelBindingContext model) =>
            InvokeAllForRoot(model, ROOT, ACTIONS);
    }
}