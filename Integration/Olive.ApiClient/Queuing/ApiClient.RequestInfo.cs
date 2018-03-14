using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive
{
    partial class ApiClient
    {
        partial class RequestInfo
        {
            /// <summary>
            /// Sends this request to the server and processes the response.
            /// The error action will also apply.
            /// It will return whether the response was successfully received.
            /// </summary>
            public async Task<bool> Send<TEntity, TIdentifier>(TEntity entity) where TEntity : IQueueable<TIdentifier>
            {
                try
                {
                    ResponseText = (
                          Task.Run(DoSend).ToString()
                        ).ToStringOrEmpty();
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("Internet connection is unavailable."))
                    {
                        // Add Queue status and properties
                        entity.RequestInfo = this;
                        entity.TimeAdded = DateTime.Now;
                        entity.Status = QueueStatus.Added;

                        // Add item to the Queue and write it to file
                        await Client.AddQueueItem<TEntity, TIdentifier>(entity);

                        // Update the response caches
                        await UpdateCacheUponOfflineModification<TEntity, TIdentifier>(entity, HttpMethod);
                        return true;
                    }

                    LogTheError(ex);
                    return false;
                }
            }

            async Task UpdateCacheUponOfflineModification<TResponse, TIdentifier>(TResponse modified, string httpMethod) where TResponse : IQueueable<TIdentifier>
            {
                await Task.Delay(50);

                // Get all cached files for this type
                var cachedFiles = GetTypeCacheFiles(modified);
                foreach (var file in cachedFiles)
                {
                    var records = await Client.DeserializeResponse<IEnumerable<TResponse>>(file).ToList();
                    var changed = false;

                    // If the file contains the modified row, update it
                    if (httpMethod == "DELETE")
                    {
                        var deletedRecords = records.Where(x => EqualityComparer<TIdentifier>.Default.Equals(x.ID, modified.ID));
                        if (deletedRecords.Any())
                        {
                            records.RemoveAll(x => deletedRecords.Contains(x));
                            changed = true;
                        }
                    }
                    else if (httpMethod == "PATCH" || httpMethod == "PUT")
                        records?.Do(record =>
                        {
                            record = modified;
                            changed = true;
                        });

                    if (!changed) continue;
                    // If cache file is edited, rewrite it
                    var newResponseText = JsonConvert.SerializeObject(records);
                    if (newResponseText.HasValue())
                        await file.WriteAllTextAsync(newResponseText);
                }
            }
        }
    }
}