using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Mvc
{
    /// <summary>
    /// Any method attributed by this which takes one IViewModel parameter, will be automatically 
    /// called when its view model parameter object is being bound.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class BaseModelBindAttribute : Attribute
    {
        protected static IEnumerable<Action> DiscoverBindMethods(ControllerContext cContext, object model, Type attributeType,
            ConcurrentDictionary<Type, List<MethodInfo>> cache)
        {
            var customBinders = cache.GetOrAdd(model.GetType(),

                  t => t.Assembly.GetTypes().Where(x => x.IsA<Controller>()).SelectMany(x => x.GetMethods())
                      .Where(m => m.GetCustomAttributes(attributeType).Any())
                      .Where(m => m.GetParameters().IsSingle() && m.GetParameters().First().ParameterType == t)
                      .ToList()
                  );

            throw new NotImplementedException("The following code is commented to fix on the test time.");
            // foreach (var customBinder in customBinders)
            // {
            //    ControllerBase controller;

            //    if (cContext.Controller.GetType().IsA(customBinder.DeclaringType))
            //        controller = cContext.Controller;
            //    else
            //    {
            //        controller = (ControllerBase)customBinder.DeclaringType.CreateInstance();
            //        (controller as Controller).Url = new UrlHelper(cContext.RequestContext);
            //        controller.ControllerContext = cContext;
            //    }

            //    Action invokeMethod = () =>
            //    {
            //        try { customBinder.Invoke(controller, new[] { model }); }
            //        catch (Exception ex)
            //        {
            //            throw new Exception($"Error in calling the binding method of {customBinder.DeclaringType.Name}.{customBinder.Name}({model.GetType().Name}).", ex);
            //        }
            //    };

            //    yield return invokeMethod;
            // }
        }

        protected static void Enqueue(ControllerContext cContext, object model, string actionsKey, Type attributeType,
            ConcurrentDictionary<Type, List<MethodInfo>> cache)
        {
            if (!(cContext.HttpContext.Items[actionsKey] is List<Action> queue))
            {
                queue = new List<Action>();
                cContext.HttpContext.Items[actionsKey] = queue;
            }

            foreach (var action in DiscoverBindMethods(cContext, model, attributeType, cache))
                queue.Insert(0, action);
        }

        protected static void SetRoot(ModelBindingContext model, string rootKey)
        {
            if (model.HttpContext.Items[rootKey] == null) model.HttpContext.Items[rootKey] = model;
        }

        protected static void InvokeAllForRoot(ModelBindingContext model, string rootKey, string actionsKey)
        {
            var httpContext = model.HttpContext;

            // Don't invoke if it's not for the root item.
            if (!ReferenceEquals(httpContext.Items[rootKey], model)) return;

            if (model.ModelType?.IsA<IViewModel>() == true)
            {
                var key = "AlreadyRan." + actionsKey + model.ModelType.FullName;

                if (httpContext.Items[key].ToStringOrEmpty() == "true") return;
                else httpContext.Items[key] = "true";
            }

            var actions = (List<Action>)httpContext.Items[actionsKey];

            httpContext.Items[rootKey] = httpContext.Items[actionsKey] = null;

            if (actions != null)
                foreach (var action in actions.ToArray()) action();
        }
    }
}