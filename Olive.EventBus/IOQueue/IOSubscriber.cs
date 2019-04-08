using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    class IOSubscriber<TMessage> where TMessage : IEventBusMessage
    {
        AsyncLock SyncLock = new AsyncLock();
        Func<TMessage, Task> Handler;
        DirectoryInfo Folder;

        public IOSubscriber(IOEventBusQueue queue, Func<TMessage, Task> handler)
        {
            Folder = queue.Folder;
            Handler = handler;
        }

        public void Start() => new Thread(KeepPolling).Start();

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

        internal static async Task<KeyValuePair<FileInfo, TMessage>> FetchOnce(DirectoryInfo folder)
        {
            var item = folder.GetFiles().OrderBy("CreationTimeUtc").FirstOrDefault();

            if (item == null) return new KeyValuePair<FileInfo, TMessage>(null, default(TMessage));

            var content = await ReadFile(item);
            try
            {
                var @event = JsonConvert.DeserializeObject<TMessage>(content);
                return new KeyValuePair<FileInfo, TMessage>(item, @event);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + content, ex);
            }
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

        async void RunHandler()
        {
            while (true)
            {
                using (await SyncLock.Lock())
                    if (await HandleNext()) continue;

                Thread.Sleep(5000);
            }
        }
    }
}