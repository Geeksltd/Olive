using System;
using System.Collections.Generic;
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

            Folder = Path.GetTempPath().AsDirectory().GetOrCreateSubDirectory($@"Olive\IO.Queue\{folder}");
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

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages)
        {
            var result = new List<string>();

            await messages.DoAsync(async (m, _) => result.Add(await Publish(m)));

            return result;
        }

        public async Task<QueueMessageHandle> Pull(int timeoutSeconds = 10)
        {
            var item = await IOSubscriber.FetchOnce(Folder);
            if (item.Key == null) return null;

            return new QueueMessageHandle(item.Value, item.Key.Name, () => { item.Key.DeleteIfExists(); return Task.CompletedTask; });
        }

        public Task Purge()
        {
            Folder.GetFiles().Do(x => x.DeleteIfExists());
            return Task.CompletedTask;
        }

        public void Subscribe(Func<string, Task> handler) => new IOSubscriber(this, handler).Start();
        public Task PullAll(Func<string, Task> handler) => new IOSubscriber(this, handler).PullAll();
    }
}