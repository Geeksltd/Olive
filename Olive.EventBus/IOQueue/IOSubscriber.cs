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

        public void Start() => new Thread(KeepPolling).Start();

        public Task PullAll() => RunHandler(PullStrategy.UntilEmpty);

        void KeepPolling()
        {
            RunHandler();

            var watcher = new FileSystemWatcher(Folder.FullName) { IncludeSubdirectories = false };
            watcher.Created += OnFoundNewFile;
            watcher.EnableRaisingEvents = true;
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

        async void OnFoundNewFile(object sender, FileSystemEventArgs e)
        {
            using (await SyncLock.Lock())
                await HandleNext();
        }

        async Task<bool> HandleNext()
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
                throw new Exception("Failed to run queue event handler " +
                    Handler.Method.DeclaringType.FullName + "." +
                    Handler.Method.GetDisplayName(), ex);
            }

            return true;
        }

        async Task RunHandler(PullStrategy strategy = PullStrategy.KeepPulling)
        {
            do
            {

                using (await SyncLock.Lock())
                    if (await HandleNext()) continue;

                if (strategy == PullStrategy.KeepPulling)
                    Thread.Sleep(5000);
                else
                    break;
            }
            while (true);
        }
    }
}