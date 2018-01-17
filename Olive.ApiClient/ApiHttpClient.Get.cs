using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.ApiClient
{
    partial class ApiHttpClient
    {
        public static string StaleDataWarning = "The latest data cannot be received from the server right now.";
        const string CACHE_FOLDER = "-ApiCache";
        static object CacheSyncLock = new object();

        static FileInfo GetCacheFile<TResponse>(string url)
        {
            lock (CacheSyncLock)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(CACHE_FOLDER, GetTypeName<TResponse>()));
                if (directoryInfo.Exists)
                {
                    return directoryInfo.GetFile(url.ToSimplifiedSHA1Hash() + ".txt");
                }
                return null;
            }
        }

        static FileInfo[] GetTypeCacheFiles<TResponse>(TResponse modified)
        {
            lock (CacheSyncLock)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(CACHE_FOLDER, GetTypeName(modified)));
                if (directoryInfo.Exists)
                {
                    return directoryInfo.GetFiles();
                }
                return null;
            }
        }

        static string GetTypeName<T>() => typeof(T).GetGenericArguments().SingleOrDefault()?.Name ?? typeof(T).Name.Replace("[]", "");

        static string GetTypeName<T>(T modified) => modified.GetType().Name;

        static string GetFullUrl(string baseUrl, object queryParams = null)
        {
            if (queryParams == null) return baseUrl;

            var queryString = queryParams as string;

            if (queryString == null)
                queryString = queryParams.GetType().GetPropertiesAndFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name + "=" + p.GetValue(queryParams).ToStringOrEmpty().UrlEncode())
                     .Trim().ToString("&");

            if (queryString.LacksAll()) return baseUrl;

            if (baseUrl.Contains("?")) return (baseUrl + "&" + queryString).KeepReplacing("&&", "&");
            return baseUrl + "?" + queryString;
        }

        static bool HasValue<TType>(TType value)
        {
            if (ReferenceEquals(value, null)) return false;
            if (value.Equals(default(TType))) return false;
            return true;
        }

        public static async Task<TResponse> Get<TResponse>(string relativeUrl, object queryParams = null, OnError errorAction = OnError.Throw, ApiResponseCache cacheChoice = ApiResponseCache.Accept, Func<TResponse, Task> refresher = null)
        {
            if (refresher != null && cacheChoice != ApiResponseCache.PreferThenUpdate)
                throw new ArgumentException("refresher can only be provided when using ApiResponseCache.PreferThenUpdate.");

            if (refresher == null && cacheChoice == ApiResponseCache.PreferThenUpdate)
                throw new ArgumentException("When using ApiResponseCache.PreferThenUpdate, refresher must be specified.");

            relativeUrl = GetFullUrl(relativeUrl, queryParams);

            var result = default(TResponse);
            if (cacheChoice == ApiResponseCache.Prefer || cacheChoice == ApiResponseCache.PreferThenUpdate)
            {
                result = GetCachedResponse<TResponse>(relativeUrl);
                if (HasValue(result))
                {
                    if (cacheChoice == ApiResponseCache.PreferThenUpdate)
                        new Thread(async () =>
                        {
                            await RefreshUponUpdatedResponse(relativeUrl, refresher);
                        }).Start();
                    return result;
                }
            }

            var request = new RequestInfo(relativeUrl) { ErrorAction = errorAction, HttpMethod = "GET" };

            if (await request.Send())
            {
                result = request.ExtractResponse<TResponse>();

                if (request.Error == null)
                    await GetCacheFile<TResponse>(relativeUrl).WriteAllTextAsync(request.ResponseText);
            }

            if (request.Error != null && cacheChoice != ApiResponseCache.Refuse)
            {
                result = GetCachedResponse<TResponse>(relativeUrl);
                //if (HasValue(result) && cacheChoice == ApiResponseCache.AcceptButWarn)
                //    await Alert.Toast(StaleDataWarning);
            }

            return result;
        }

        static async Task RefreshUponUpdatedResponse<TResponse>(string url, Func<TResponse, Task> refresher)
        {
            await Task.Delay(50);

            string localCachedVersion;
            try
            {
                localCachedVersion = (await GetCacheFile<TResponse>(url).ReadAllTextAsync()).CreateSHA1Hash();
                if (localCachedVersion.LacksAll()) throw new Exception("Local cached file's hash is empty!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Strangely, there is no cache any more when running RefreshUponUpdatedResponse(...).");
                Debug.WriteLine(ex);
                return; // High concurrency perhaps.
            }

            var request = new RequestInfo(url)
            {
                ErrorAction = OnError.Throw,
                HttpMethod = "GET",
                LocalCachedVersion = localCachedVersion
            };

            try
            {
                if (!await request.Send()) return;

                if (localCachedVersion.HasValue() && request.ResponseCode == System.Net.HttpStatusCode.NotModified) return;

                var newResponseCache = request.ResponseText.OrEmpty().CreateSHA1Hash();
                if (newResponseCache == localCachedVersion)
                {
                    // Same response. No update needed.
                    return;
                }

                var result = request.ExtractResponse<TResponse>();
                if (request.Error == null)
                {
                    await GetCacheFile<TResponse>(url).WriteAllTextAsync(request.ResponseText);
                    await refresher(result);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        static async Task UpdateCacheUponOfflineModification<TResponse, TIdentifier>(TResponse modified, string httpMethod) where TResponse : IQueueable<TIdentifier>
        {
            await Task.Delay(50);

            // Get all cached files for this type
            var cachedFiles = GetTypeCacheFiles(modified);
            foreach (var file in cachedFiles)
            {
                var records = DeserializeResponse<IEnumerable<TResponse>>(file).ToList();
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

        static TResponse GetCachedResponse<TResponse>(string url)
        {
            var file = GetCacheFile<TResponse>(url);
            return DeserializeResponse<TResponse>(file);
        }

        static TResponse DeserializeResponse<TResponse>(FileInfo file)
        {
            if (!file.Exists()) return default(TResponse);

            try
            {
                return JsonConvert.DeserializeObject<TResponse>(
                    file.ReadAllText(),
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    }
                );
            }
            catch { return default(TResponse); }
        }

        /// <summary>
        /// Deletes all cached Get API results.
        /// </summary>
        public static Task DisposeCache()
        {
            lock (CacheSyncLock)
            {
                if (Directory.Exists(CACHE_FOLDER))
                    Directory.Delete(CACHE_FOLDER, true);
            }

            // Desined as a task in case in the future we need it.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes the cached Get API result for the specified API url.
        /// </summary>
        public static Task DisposeCache<TResponse>(string getApiUrl)
        {
            lock (CacheSyncLock)
            {
                var file = GetCacheFile<TResponse>(getApiUrl);
                if (file.Exists()) file.DeleteAsync(true);
            }

            return Task.CompletedTask;
        }
    }
}