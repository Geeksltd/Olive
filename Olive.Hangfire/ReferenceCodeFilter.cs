using System;
using Hangfire.Common;
using Hangfire.Server;

namespace Olive.Hangfire
{
    /// <summary>
    /// Gives every job run a reference code, so that everything it logs can be found together — the
    /// same grouping the reference code middleware gives an HTTP request. A server filter rather than a
    /// wrapper at registration, because BackgroundJob.Action is a serialized Expression with nowhere to
    /// put a closure.
    /// </summary>
    class ReferenceCodeFilter : JobFilterAttribute, IServerFilter
    {
        const string Key = "Olive.ReferenceScope";

        // Null: a job is nobody's request, so it starts a code of its own rather than adopting one.
        public void OnPerforming(PerformingContext context) => context.Items[Key] = Log.UseReference(null);

        public void OnPerformed(PerformedContext context)
        {
            if (context.Items.TryGetValue(Key, out var scope)) (scope as IDisposable)?.Dispose();
        }
    }
}
