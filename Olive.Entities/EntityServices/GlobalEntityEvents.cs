using System;
using System.ComponentModel;

namespace Olive.Entities
{
    public static class GlobalEntityEvents
    {
        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<CancelEventArgs> InstanceSaving = new AsyncEvent<CancelEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<SaveEventArgs> InstanceSaved = new AsyncEvent<SaveEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<CancelEventArgs> InstanceDeleting = new AsyncEvent<CancelEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is validated.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// This will be called as the first line of the base Entity's OnValidating method.
        /// </summary>
        public readonly static AsyncEvent<EventArgs> InstanceValidating = new AsyncEvent<EventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<EventArgs> InstanceDeleted = new AsyncEvent<EventArgs>();
    }
}