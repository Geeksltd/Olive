using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    partial class BindAttributeRunner
    {
        class ViewModelBinding
        {
            static ConcurrentDictionary<string, MethodInfo[]> PreBindingCache = new ConcurrentDictionary<string, MethodInfo[]>();
            static ConcurrentDictionary<string, MethodInfo[]> PreBoundCache = new ConcurrentDictionary<string, MethodInfo[]>();
            static ConcurrentDictionary<string, MethodInfo[]> BoundCache = new ConcurrentDictionary<string, MethodInfo[]>();

            public IViewModel Model;
            public Controller[] Controllers;
            public bool RanPreBinding, RanPreBound, RanBound;

            public async Task Run<TAttribute>() where TAttribute : Attribute
            {
                foreach (var controller in Controllers)
                {
                    var methods = FindMethods<TAttribute>(controller, Model);
                    await InvokeMethods<TAttribute>(methods, controller, Model);
                }
            }

            static MethodInfo[] FindMethods<TAtt>(Controller controller, object model) where TAtt : Attribute
            {
                var key = controller.GetType().FullName + "|" + model.GetType().FullName;
                return GetCache<TAtt>().GetOrAdd(key,
                      t =>
                      {
                          return controller.GetType().GetMethods()
                           .Where(m => m.Defines<TAtt>())
                           .Where(m => m.GetParameters().IsSingle())
                           .Where(m => m.GetParameters().First().ParameterType == model.GetType())
                           .ToArray();
                      });
            }

            Task InvokeMethods<TAttribute>(MethodInfo[] methods, Controller controller, object viewModel)
                 where TAttribute : Attribute
            {
                var tasks = new List<Task>();

                foreach (var info in viewModel.GetType().GetProperties())
                {
                    if (!info.CanWrite) continue;
                    if (info.PropertyType.IsA<IViewModel>())
                    {
                        var nestedValue = info.GetValue(viewModel);
                        if (nestedValue != null)
                        {
                            var binders = FindMethods<TAttribute>(controller, nestedValue);
                            if (binders.Any())
                                tasks.Add(InvokeMethods<TAttribute>(binders, controller, nestedValue));
                        }
                    }
                }

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
        }
    }
}