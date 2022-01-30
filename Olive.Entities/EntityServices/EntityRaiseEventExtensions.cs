using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public static class EntityRaiseEventExtensions
    {
        public static async Task RaiseOnDeleting(this EntityServices @this, IEntity record, CancelEventArgs args)
        {
            await GlobalEntityEvents.OnInstanceDeleting(args).ConfigureAwait(false);
            if (args.Cancel) return;
            await ((Entity)record).OnDeleting(args).ConfigureAwait(false);
        }

        public static async Task RaiseOnValidating(this EntityServices @this, IEntity record, EventArgs args)
        {
            await GlobalEntityEvents.OnInstanceValidating(args).ConfigureAwait(false);
            await ((Entity)record).OnValidating(args).ConfigureAwait(false);
        }

        public static Task RaiseOnDeleted(this EntityServices @this, IEntity record)
        {
            return ((Entity)record).OnDeleted(EventArgs.Empty);
        }

        public static async Task RaiseOnLoaded(this EntityServices @this, IEntity record)
        {
            await ((Entity)record).OnLoaded().ConfigureAwait(false);

            foreach (var item in Context.Current.GetServices<IEntityLoadedInterceptor>())
            {
                var task = item.Process(record);
                if (task != null) await task.ConfigureAwait(false);
            }
        }

        public static async Task RaiseOnSaving(this EntityServices @this, IEntity record, CancelEventArgs e)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await GlobalEntityEvents.OnInstanceSaving(e).ConfigureAwait(false);
            if (e.Cancel) return;

            await (record as Entity).OnSaving(e).ConfigureAwait(false);

            foreach (var item in Context.Current.GetServices<IEntitySavingInterceptor>())
            {
                var task = item.Process(record);
                if (task != null) await task.ConfigureAwait(false);
            }
        }

        public static Task RaiseOnSaved(this EntityServices @this, IEntity record, SaveEventArgs e)
        {
            return ((Entity)record).OnSaved(e);
        }
    }
}