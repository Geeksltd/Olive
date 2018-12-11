using Newtonsoft.Json;
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

        public async Task<string> Publish(IEventBusMessage message)
        {
            using (await SyncLock.Lock())
                await Task.Delay(5.Milliseconds()); // Ensure the file names are unique.

            var path = Folder.GetFile(DateTime.UtcNow.Ticks.ToString());
            await path.WriteAllTextAsync(JsonConvert.SerializeObject(message));
            return path.Name;
        }

        public Task Purge()
        {
            Folder.GetFiles().Do(x => x.DeleteIfExists());
            return Task.CompletedTask;
        }

        public void Subscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventBusMessage
        {
            new IOSubscriber<TMessage>(this, handler).Start();
        }
    }
}