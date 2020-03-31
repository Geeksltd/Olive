using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public static class EntityRaiseEventExtensions
    {
        public static async Task RaiseOnDeleting(this EntityServices @this, IEntity record, CancelEventArgs args)
        {
            await GlobalEntityEvents.OnInstanceDeleting(args);
            if (args.Cancel) return;
            await ((Entity)record).OnDeleting(args);
        }

        public static async Task RaiseOnValidating(this EntityServices @this, IEntity record, EventArgs args)
        {
            await GlobalEntityEvents.OnInstanceValidating(args);
            await ((Entity)record).OnValidating(args);
        }

        public static async Task RaiseOnDeleted(this EntityServices @this, IEntity record)
        {
            await ((Entity)record).OnDeleted(EventArgs.Empty);
        }

        public static async Task RaiseOnLoaded(this EntityServices @this, IEntity record)
        {
            await ((Entity)record).OnLoaded();
            foreach (var item in Context.Current.GetServices<IEntityLoadedInterceptor>())
            {
                var task = item.Process(record);
                if (task != null) await task;
            }
        }

        public static async Task RaiseOnSaving(this EntityServices @this, IEntity record, CancelEventArgs e)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await GlobalEntityEvents.OnInstanceSaving(e);
            if (e.Cancel) return;

            await (record as Entity).OnSaving(e);

            foreach (var item in Context.Current.GetServices<IEntitySavingInterceptor>())
            {
                var task = item.Process(record);
                if (task != null) await task;
            }
        }

        public static async Task RaiseOnSaved(this EntityServices @this, IEntity record, SaveEventArgs e)
        {
            await ((Entity)record).OnSaved(e);
        }
    }
}