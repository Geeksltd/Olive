using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc
{
    public static class BindAttributeRunner
    {
        static ConcurrentDictionary<string, MethodInfo[]> PreBindingCache = new ConcurrentDictionary<string, MethodInfo[]>();
        static ConcurrentDictionary<string, MethodInfo[]> PreBoundCache = new ConcurrentDictionary<string, MethodInfo[]>();
        static ConcurrentDictionary<string, MethodInfo[]> BoundCache = new ConcurrentDictionary<string, MethodInfo[]>();

        public static Task Run(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var args = context.ActionArguments.Select(x => x.Value).OfType<IViewModel>().ToArray();
            return BindOn(context.Controller, args);
        }

        public static async Task BindOn(object controller, params IViewModel[] items)
        {
            foreach (var item in items)
                await Run<OnPreBindingAttribute>(item, controller);

            foreach (var item in items)
                await Run<OnPreBoundAttribute>(item, controller);

            foreach (var item in items)
                await Run<OnBoundAttribute>(item, controller);
        }

        internal static async Task Run<TAttribute>(IViewModel viewModel, object controller)
        where TAttribute : Attribute
        {
            var methods = FindMethods<TAttribute>(viewModel, controller);
            await InvokeMethods(methods, controller, viewModel);
        }

        static MethodInfo[] FindMethods<TAtt>(IViewModel viewModel, object controller)
            where TAtt : Attribute
        {
            var key = GetKey(controller, viewModel);
            return GetCache<TAtt>().GetOrAdd(key,
                  t =>
                  {
                      var methods = controller.GetType().GetMethods().Where(m => m.Defines<TAtt>()).ToArray();

                      methods = methods.Where(m => m.GetParameters().IsSingle()
                      && m.GetParameters().First().ParameterType == viewModel.GetType()).ToArray();

                      return methods;
                  });
        }

        static Task InvokeMethods(MethodInfo[] methods, object controller, object viewModel)
        {
            foreach (var info in viewModel.GetType().GetProperties())
            {
                var nestedValue = info.GetValue(viewModel);
                if (info.PropertyType.IsA<IViewModel>() && nestedValue != null)
                    InvokeMethods(methods, controller, nestedValue);
            }

            var tasks = new List<Task>();

            foreach (var method in methods)
            {
                var result = method.Invoke(controller, new[] { viewModel });
                if (result is Task task) tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }

        static ConcurrentDictionary<string, MethodInfo[]> GetCache<TAttribute>()
        {
            if (typeof(TAttribute) == typeof(OnPreBindingAttribute)) return PreBindingCache;
            else if (typeof(TAttribute) == typeof(OnPreBoundAttribute)) return PreBoundCache;
            else if (typeof(TAttribute) == typeof(OnBoundAttribute)) return BoundCache;
            else throw new NotSupportedException(typeof(TAttribute) + " is not supported!!");
        }

        static string GetKey(object controller, IViewModel viewModel)
        {
            return controller.GetType().FullName + "|" + viewModel.GetType().FullName;
        }
    }
}