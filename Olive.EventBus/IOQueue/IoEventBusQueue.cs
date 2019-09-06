using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    class IOEventBusQueue : IEventBusQueue
    {
        internal DirectoryInfo Folder;
        AsyncLock SyncLock = new AsyncLock();

        public IOEventBusQueue(string queueUrl)
        {
            var folder = queueUrl.TrimStart("https:").TrimStart("http:").TrimStart("//")
                 .Select(x => Path.GetInvalidFileNameChars().Contains(x) ? '_' : x)
                 .ToString("").KeepReplacing("__", "_");

            Folder = Path.Combine(Path.GetTempPath(), $@"Olive\IO.Queue\{folder}").AsDirectory().EnsureExists();
        }

        public async Task<string> Publish(string message)
        {
            FileInfo path;
            using (await SyncLock.Lock())
            {
                path = Folder.GetFile(DateTime.UtcNow.Ticks.ToString());
            }
            await path.WriteAllTextAsync(message);
            return path.Name;
        }

        public async Task<QueueMessageHandle<string>> Pull(int timeoutSeconds = 10)
        {
            var item = await IOSubscriber.FetchOnce(Folder);
            if (item.Key == null) return null;

            return new QueueMessageHandle<string>(item.Value, () => { item.Key.DeleteIfExists(); return Task.CompletedTask; });
        }

        public Task Purge()
        {
            Folder.GetFiles().Do(x => x.DeleteIfExists());
            return Task.CompletedTask;
        }

        public void Subscribe(Func<string, Task> handler) => new IOSubscriber(this, handler).Start();
    }
}