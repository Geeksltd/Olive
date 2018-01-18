using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive.ApiClient
{
    partial class ApiHttpClient
    {
        const string QUEUE_FOLDER = "-ApiQueue";
        static object QueueSyncLock = new object();

        static FileInfo GetQueueFile()
        {
            lock (QueueSyncLock)
            {
                var file = new DirectoryInfo(QUEUE_FOLDER).GetFile("Queue.txt");
                // TODO: not sure why needed for the first time. Should be removed in future.
                if (!file?.Exists ?? true) file.WriteAllText("");
                return file;
            }
        }

        static bool UpdateQueueFile<TEntity, TIdentifier>(IEnumerable<TEntity> items) where TEntity : IQueueable<TIdentifier>
        {
            var text = JsonConvert.SerializeObject(items);
            if (text.HasValue())
            {
                lock (QueueSyncLock)
                    GetQueueFile().WriteAllText(text);
                return true;
            }

            return false;
        }

        public static async Task<IEnumerable<TEntity>> GetQueueItems<TEntity>()
        {
            var file = GetQueueFile();
            var text = await file.ReadAllTextAsync();
            return JsonConvert.DeserializeObject<IEnumerable<TEntity>>(
                    text,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
        }

        static async Task<bool> UpdateQueueItem<TEntity, TIdentifier>(TEntity item) where TEntity : IQueueable<TIdentifier>
        {
            var queueItems = await GetQueueItems<TEntity>();
            var edited = false;

            queueItems?.Do(queueItem =>
            {
                if (EqualityComparer<TIdentifier>.Default.Equals(queueItem.ID, item.ID))
                {
                    queueItem = item;
                    edited = true;
                }
            });

            if (edited)
                return UpdateQueueFile<TEntity, TIdentifier>(queueItems);

            return false;
        }

        static async Task<bool> AddQueueItem<TEntity, TIdentifier>(TEntity item) where TEntity : IQueueable<TIdentifier>
        {
            var queueItems = (await GetQueueItems<TEntity>() ?? new List<TEntity>()).ToList();
            queueItems.Add(item);
            return UpdateQueueFile<TEntity, TIdentifier>(queueItems);
        }

        public static async Task<bool> ApplyQueueItems<TEntity, TIdentifier>() where TEntity : IQueueable<TIdentifier>
        {
            var queueItems = await GetQueueItems<TEntity>();

            queueItems?.Select(async queueItem =>
              {
                  if (queueItem.Status == QueueStatus.Added)
                  {
                      if (await queueItem.RequestInfo.Send()) queueItem.Status = QueueStatus.Applied;
                      else queueItem.Status = QueueStatus.Rejected;
                      queueItem.TimeUpdated = DateTime.Now;
                  }
              });

            return UpdateQueueFile<TEntity, TIdentifier>(queueItems);
        }
    }
}