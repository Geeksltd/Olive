using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public partial class BindAttributeRunner
    {


        readonly Controller RootController;
        readonly Dictionary<IViewModel, ViewModelBinding> ViewModels = new Dictionary<IViewModel, ViewModelBinding>();


        public BindAttributeRunner(Controller rootController, IViewModel[] models)
        {
            RootController = rootController;
            foreach (var item in models)
                Add(item);
        }

        bool Add(IViewModel viewModel)
        {
            if (ViewModels.ContainsKey(viewModel)) return false;
            ViewModels.Add(viewModel, new ViewModelBinding { Model = viewModel, Controllers = GetControllers(viewModel).ToArray() });
            Expand(viewModel);
            return true;
        }

        public static Task Run(ActionExecutingContext context)
        {
            var args = context.ActionArguments.Select(x => x.Value).OfType<IViewModel>().ToArray();
            return new BindAttributeRunner((Controller)context.Controller, args).Execute();
        }

        List<IViewModel> ExpandAll() => ViewModels.ToArray().SelectMany(x => Expand(x.Key)).ToList();

        List<IViewModel> Expand(IViewModel model)
        {
            var added = new List<IViewModel>();
            var properties = model.GetType().GetPropertiesAndFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var p in properties.Where(v => v.GetPropertyOrFieldType().IsA<IViewModel>()))
            {
                var obj = p.GetValue(model) as IViewModel;
                if (obj != null)
                    if (Add(obj)) added.Add(obj);
            }

            foreach (var p in properties.Where(v => v.GetPropertyOrFieldType().IsIEnumerableOf(typeof(IViewModel))))
            {
                var objs = p.GetValue(model) as IEnumerable<IViewModel>;
                foreach (var obj in objs.OrEmpty().ExceptNull())
                    if (Add(obj)) added.Add(obj);
            }

            return added;
        }

        public static Task Bind(IViewModel item, Controller controller)
        {
            if (item is null) return Task.CompletedTask;
            return new BindAttributeRunner(controller, new[] { item }).Execute();
        }

        IEnumerable<Controller> GetControllers(IViewModel item)
        {
            yield return RootController;

            var type = item?.GetType().GetCustomAttribute<BindingControllerAttribute>()?.Type;
            if (type != null)
            {
                var result = type.CreateInstanceWithDI() as Controller;
                result.ControllerContext = RootController.ControllerContext;
                yield return result;
            }
        }

        async Task Execute()
        {
            await ExecutePreBinding();
            await ExecutePreBound();
            await ExecuteBound();
        }

        async Task ExecutePreBinding()
        {
            foreach (var item in ViewModels.Values.Except(x => x.RanPreBinding).ToArray())
            {
                item.RanPreBinding = true;
                await item.Run<OnPreBindingAttribute>();
            }

            if (ExpandAll().Any())
                await ExecutePreBinding();
        }

        async Task ExecutePreBound()
        {
            foreach (var item in ViewModels.Values.Except(x => x.RanPreBound).ToArray())
            {
                item.RanPreBound = true;
                await item.Run<OnPreBoundAttribute>();
            }

            var remaining = ExpandAll();
            if (remaining.Any())
            {
                await ExecutePreBinding();
                await ExecutePreBound();
            }
        }

        async Task ExecuteBound()
        {
            foreach (var item in ViewModels.Values.Except(x => x.RanBound).ToArray())
            {
                item.RanBound = true;
                await item.Run<OnBoundAttribute>();
            }

            var remaining = ExpandAll();
            if (remaining.Any())
            {
                await ExecutePreBinding();
                await ExecutePreBound();
                await ExecuteBound();
            }
        }




    }
}