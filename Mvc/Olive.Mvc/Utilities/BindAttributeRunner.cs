using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public static class BindAttributeRunner
    {
        static ConcurrentDictionary<string, MethodInfo[]> PreBindingCache = new ConcurrentDictionary<string, MethodInfo[]>();
        static ConcurrentDictionary<string, MethodInfo[]> PreBoundCache = new ConcurrentDictionary<string, MethodInfo[]>();
        static ConcurrentDictionary<string, MethodInfo[]> BoundCache = new ConcurrentDictionary<string, MethodInfo[]>();

        public static Task Run(ActionExecutingContext context)
        {
            var args = context.ActionArguments.Select(x => x.Value).OfType<IViewModel>().ToArray();

            var items = args.Select(x => KeyValuePair.Create(x,
                GetControllers(x, (Controller)context.Controller).ToArray())).ToArray();

            return BindOn(items);
        }

        public static Task Bind(IViewModel item, Controller controller)
        {
            if (item is null) return Task.CompletedTask;
            return BindOn(KeyValuePair.Create(item, GetControllers(item, controller).ToArray()));
        }

        static IEnumerable<Controller> GetControllers(IViewModel item, Controller controller)
        {
            yield return controller;

            var type = item?.GetType().GetCustomAttribute<BindingControllerAttribute>()?.Type;
            if (type != null)
            {
                var result = type.CreateInstance() as Controller;
                result.ControllerContext = controller.ControllerContext;
                yield return result;
            }
        }

        static async Task BindOn(params KeyValuePair<IViewModel, Controller[]>[] items)
        {
            foreach (var item in items)
                await Run<OnPreBindingAttribute>(item.Key, item.Value);

            foreach (var item in items)
                await Run<OnPreBoundAttribute>(item.Key, item.Value);

            foreach (var item in items)
                await Run<OnBoundAttribute>(item.Key, item.Value);
        }

        internal static async Task Run<TAttribute>(IViewModel viewModel, Controller[] controllers)
        where TAttribute : Attribute
        {
            foreach (var controller in controllers)
            {
                var methods = FindMethods<TAttribute>(viewModel, controller);
                await InvokeMethods(methods, controller, viewModel);
            }
        }

        static MethodInfo[] FindMethods<TAtt>(IViewModel viewModel, Controller controller)
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

        static Task InvokeMethods(MethodInfo[] methods, Controller controller, object viewModel)
        {
            foreach (var info in viewModel.GetType().GetProperties())
            {
                if (!info.CanWrite) continue;
                if (info.PropertyType.IsA<IViewModel>())
                {
                    var nestedValue = info.GetValue(viewModel);
                    if (nestedValue != null)
                        InvokeMethods(methods, controller, nestedValue);
                }
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

        static string GetKey(Controller controller, IViewModel viewModel)
        {
            return controller.GetType().FullName + "|" + viewModel.GetType().FullName;
        }
    }
}