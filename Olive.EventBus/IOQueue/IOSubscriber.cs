using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    class IOSubscriber
    {
        AsyncLock SyncLock = new AsyncLock();
        Func<string, Task> Handler;
        DirectoryInfo Folder;

        public IOSubscriber(IOEventBusQueue queue, Func<string, Task> handler)
        {
            Folder = queue.Folder;
            Handler = handler;
        }

        /// <summary>The development equivalent of a dead-letter queue. A sub-directory, so GetFiles()
        /// never picks it up as a message.</summary>
        const string FailedFolderName = "failed";

        public void Start() => new Thread(KeepPolling).Start();

        public Task PullAll() => RunHandler(PullStrategy.UntilEmpty);

        void KeepPolling()
        {
            // RunHandler is async, so anything escaping it faults its Task rather than this thread.
            // Unobserved, that ends the loop silently for the life of the process.
            RunHandler().ContinueWith(
                task => Report("The development queue subscriber for " + Folder.FullName + " has stopped. " +
                    "Messages will accumulate there until the process is restarted.", task.Exception),
                TaskContinuationOptions.OnlyOnFaulted);

            //var watcher = new FileSystemWatcher(Folder.FullName) { IncludeSubdirectories = false };
            //watcher.Created += OnFoundNewFile;
            //watcher.EnableRaisingEvents = true;
        }

        /// <summary>Says it on the console as well as in the log: this runs in development, where the
        /// logger may not be initialised yet.</summary>
        static void Report(string message, Exception error)
        {
            Console.WriteLine(message);
            if (error != null) Console.WriteLine(error.ToFullMessage());

            try { Log.For<IOSubscriber>().Error(error, message); }
            catch (Exception) { /* No logger yet, or none configured. The console has it. */ }
        }

        static async Task<string> ReadFile(FileInfo item)
        {
            while (true)
                try
                {
                    return await item.ReadAllTextAsync();
                }
                catch (System.IO.IOException)
                {
                }
        }

        internal static async Task<KeyValuePair<FileInfo, string>> FetchOnce(DirectoryInfo folder)
        {
            var item = folder.GetFiles().OrderBy("CreationTimeUtc").FirstOrDefault();

            if (item == null) return new KeyValuePair<FileInfo, string>(null, null);

            var content = await ReadFile(item);
            return new KeyValuePair<FileInfo, string>(item, content);
        }

        //async void OnFoundNewFile(object sender, FileSystemEventArgs e)
        //{
        //    using (await SyncLock.Lock())
        //        await HandleNext();
        //}

        async Task<bool> HandleNext(bool quarantineFailures)
        {
            var item = await FetchOnce(Folder);
            if (item.Key == null) return false;

            try
            {
                await Handler(item.Value);
                await item.Key.DeleteAsync(harshly: true);
            }
            catch (Exception ex)
            {
                var error = new Exception("Failed to run queue event handler " +
                    Handler.Method.DeclaringType.FullName + "." +
                    Handler.Method.GetDisplayName(), ex);

                // PullAll's caller must be told, and the message left for the next pull. Only the
                // polling loop quarantines, having nobody to tell and nothing to stop for.
                if (!quarantineFailures) throw error;

                Quarantine(item.Key, error);
            }

            return true;
        }

        /// <summary>
        /// Moves a message the handler could not process out of the way, so that one bad message costs
        /// itself and nothing else rather than failing on every poll for ever.
        /// </summary>
        void Quarantine(FileInfo message, Exception error)
        {
            Report("Development queue message " + message.Name + " in " + Folder.Name + " could not be " +
                "handled, and has been moved to the '" + FailedFolderName + "' folder. It will not be " +
                "retried. This is a fault in the handler or in whatever produced the message.", error);

            try
            {
                var target = Folder.GetOrCreateSubDirectory(FailedFolderName).GetFile(message.Name);

                // MoveTo will not overwrite and netstandard2.0 has no overload that does. Names are tick
                // counts, so a clash means the same message quarantined twice.
                target.DeleteIfExists();

                message.MoveTo(target.FullName);
            }
            catch (Exception moveError)
            {
                // It stays put, so the next poll meets it again. Noisy, but the loop still runs.
                Report("Could not quarantine development queue message " + message.FullName + ".", moveError);
            }
        }

        async Task RunHandler(PullStrategy strategy = PullStrategy.KeepPulling)
        {
            var keepPulling = strategy == PullStrategy.KeepPulling;

            do
            {
                using (await SyncLock.Lock())
                    if (await HandleNext(quarantineFailures: keepPulling)) continue;

                if (!keepPulling) break;

                // Task.Delay, not Thread.Sleep: sleeping here holds a thread-pool thread for the life
                // of the process, once per subscribed queue.
                await Task.Delay(5000);
            }
            while (true);
        }
    }
}