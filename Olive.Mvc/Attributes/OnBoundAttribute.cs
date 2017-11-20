using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    /// <summary>
    /// Any method attributed by this which takes one IViewModel parameter, will be automatically called when its view model parameter object is being bound.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnBoundAttribute : BaseModelBindAttribute
    {
        static ConcurrentDictionary<Type, List<MethodInfo>> CustomBindMethods = new ConcurrentDictionary<Type, List<MethodInfo>>();

        const string ROOT = "_Olive.CustomBindActions.RootBinder";
        const string ACTIONS = "_Olive.CustomBindActions";

        internal static void Enqueue(ControllerContext cContext, object model) =>
            Enqueue(cContext, model, ACTIONS, typeof(OnBoundAttribute), CustomBindMethods);

        internal static void SetRoot(ModelBindingContext model) => SetRoot(model, ROOT);

        internal static void InvokeAllForRoot(ModelBindingContext model) =>
            InvokeAllForRoot(model, ROOT, ACTIONS);
    }
}