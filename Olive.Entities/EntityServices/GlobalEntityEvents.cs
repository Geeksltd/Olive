using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public static class GlobalEntityEvents
    {
        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public static event AwaitableEventHandler<CancelEventArgs> InstanceSaving;

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public static event AwaitableEventHandler<GlobalSaveEventArgs> InstanceSaved;

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public static event AwaitableEventHandler<CancelEventArgs> InstanceDeleting;

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is validated.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// This will be called as the first line of the base Entity's OnValidating method.
        /// </summary>
        public static event AwaitableEventHandler<EventArgs> InstanceValidating;

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public static event AwaitableEventHandler<GlobalDeleteEventArgs> InstanceDeleted;

        internal static Task OnInstanceDeleted(GlobalDeleteEventArgs args) => InstanceDeleted.Raise(args);
        internal static Task OnInstanceDeleting(CancelEventArgs args) => InstanceDeleting.Raise(args);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task OnInstanceSaved(GlobalSaveEventArgs args) => InstanceSaved.Raise(args);
        internal static Task OnInstanceValidating(EventArgs args) => InstanceValidating.Raise(args);
        internal static Task OnInstanceSaving(CancelEventArgs args) => InstanceSaving.Raise(args);
    }
}