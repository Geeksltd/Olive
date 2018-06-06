using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive
{
    partial class ApiClient
    {
        static ConcurrentDictionary<string, AsyncLock> GetLocks =
            new ConcurrentDictionary<string, AsyncLock>();

        public string StaleDataWarning = "The latest data cannot be received from the server right now.";
        const string CACHE_FOLDER = "-ApiCache";
        static object CacheSyncLock = new object();

        CachePolicy CachePolicy = CachePolicy.FreshOrCacheOrFail;
        TimeSpan? CacheExpiry;

        public ApiClient Cache(CachePolicy policy, TimeSpan? cacheExpiry = null)
        {
            CachePolicy = policy;
            CacheExpiry = cacheExpiry;
            return this;
        }

        FileInfo GetCacheFile<TResponse>() => GetCacheFile<TResponse>(Url);

        static FileInfo GetCacheFile<TResponse>(string url)
        {
            lock (CacheSyncLock)
            {
                return GetCacheDirectory<TResponse>().GetFile(url.ToSimplifiedSHA1Hash() + ".txt");
            }
        }

        static DirectoryInfo GetCacheDirectory<TResponse>()
        {
            return GetRootCacheDirectory().GetOrCreateSubDirectory(GetTypeName<TResponse>());
        }

        static DirectoryInfo GetRootCacheDirectory()
        {
            var appName = AppDomain.CurrentDomain.BaseDirectory.AsDirectory().Parent.Name;
            return Path.Combine(Path.GetTempPath(), appName, CACHE_FOLDER).AsDirectory().EnsureExists();
        }

        static FileInfo[] GetTypeCacheFiles<TResponse>(TResponse modified)
        {
            lock (CacheSyncLock)
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(CACHE_FOLDER, GetTypeName(modified)));
                if (directoryInfo.Exists)
                {
                    return directoryInfo.GetFiles();
                }

                return null;
            }
        }

        static string GetTypeName<T>()
            => typeof(T).GetGenericArguments().SingleOrDefault()?.Name ?? typeof(T).Name.Replace("[]", "");

        static string GetTypeName<T>(T modified) => modified.GetType().Name;

        string GetFullUrl(object queryParams = null)
        {
            if (queryParams == null) return Url;

            var queryString = queryParams as string;

            if (queryString == null)
                queryString = queryParams.GetType().GetPropertiesAndFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name + "=" + p.GetValue(queryParams).ToStringOrEmpty().UrlEncode())
                     .Trim().ToString("&");

            if (queryString.LacksAll()) return Url;

            if (Url.Contains("?")) return (Url + "&" + queryString).KeepReplacing("&&", "&");
            return Url + "?" + queryString;
        }

        bool HasValue<TType>(TType value)
        {
            if (ReferenceEquals(value, null)) return false;
            if (value.Equals(default(TType))) return false;
            return true;
        }

        public async Task<TResponse> Get<TResponse>(object queryParams = null)
        {
            Url = GetFullUrl(queryParams);
            Log.For(this).Debug("Get: Url = " + Url);

            var urlLock = GetLocks.GetOrAdd(Url, x => new AsyncLock());

            using (await urlLock.Lock())
            {
                if (CachePolicy == CachePolicy.CacheOrFreshOrFail)
                {
                    var result = await GetCachedResponse<TResponse>();
                    if (HasValue(result))
                    {
                        Log.For(this).Debug("Get: Returning from Cache: " + result);
                        return result;
                    }
                }

                // Not already cached:
                return await ExecuteGet<TResponse>();
            }
        }

        async Task<TResponse> ExecuteGet<TResponse>()
        {
            var result = default(TResponse);
            if (CachePolicy == CachePolicy.CacheOrFreshOrFail)
            {
                result = await GetCachedResponse<TResponse>();
                if (HasValue(result))
                {
                    Log.For(this).Debug("ExecuteGet: Returning from Cache: " + result);
                    return result;
                }
            }

            var request = new RequestInfo(this) { HttpMethod = "GET" };

            if (await request.Send())
            {
                result = request.ExtractResponse<TResponse>();

                if (request.Error == null)
                {
                    await GetCacheFile<TResponse>().WriteAllTextAsync(request.ResponseText);
                }
            }

            if (request.Error != null)
            {
                if (CachePolicy == CachePolicy.FreshOrFail)
                    throw request.Error;

                result = await GetCachedResponse<TResponse>();
                if (result == null) // No cache available
                    throw request.Error;
            }

            return result;
        }

        // async Task RefreshUponUpdatedResponse<TResponse>(Func<TResponse, Task> refresher)
        // {
        //    await Task.Delay(50);

        //    string localCachedVersion;
        //    try
        //    {
        //        localCachedVersion = (await GetCacheFile<TResponse>().ReadAllTextAsync()).CreateSHA1Hash();
        //        if (localCachedVersion.IsEmpty())
        //            throw new Exception("Local cached file's hash is empty!");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Strangely, there is no cache any more when running RefreshUponUpdatedResponse(...).");
        //        Debug.WriteLine(ex);
        //        return; // High concurrency perhaps.
        //    }

        //    Header(x => x.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue($"\"{localCachedVersion}\"")));

        //    var request = new RequestInfo(this)
        //    {
        //        HttpMethod = "GET",
        //        LocalCachedVersion = localCachedVersion
        //    };

        //    try
        //    {
        //        if (!await request.Send()) return;

        //        if (localCachedVersion.HasValue() && request.ResponseCode == System.Net.HttpStatusCode.NotModified) return;

        //        var newResponseCache = request.ResponseText.OrEmpty().CreateSHA1Hash();
        //        if (newResponseCache == localCachedVersion)
        //        {
        //            // Same response. No update needed.
        //            return;
        //        }

        //        var result = request.ExtractResponse<TResponse>();
        //        if (request.Error == null)
        //        {
        //            await GetCacheFile<TResponse>().WriteAllTextAsync(request.ResponseText);
        //            await refresher(result);
        //        }
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex); }
        // }

        async Task<TResponse> GetCachedResponse<TResponse>()
        {
            var file = GetCacheFile<TResponse>();
            return await DeserializeResponse<TResponse>(file);
        }

        static ConcurrentDictionary<string, object> DeserializedCache =
            new ConcurrentDictionary<string, object>();

        async Task<TResponse> DeserializeResponse<TResponse>(FileInfo file)
        {
            if (!file.Exists() || IsCacheExpired(file)) return default(TResponse);

            var cacheKey = file.FullName + "|" + typeof(TResponse).FullName;
            if (CachePolicy == CachePolicy.CacheOrFreshOrFail)
            {
                // Already cached in memory?                
                if (DeserializedCache.TryGetValue(cacheKey, out var result))
                    return (TResponse)result;
            }

            try
            {
                var result = JsonConvert.DeserializeObject<TResponse>(
                    await file.ReadAllTextAsync(),
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }
                );

                if (CachePolicy == CachePolicy.CacheOrFreshOrFail)
                    DeserializedCache.TryAdd(cacheKey, result);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to deserialize the cached file into " +
                    typeof(TResponse).Name + " : " + ex.Message);

                return default(TResponse);
            }
        }

        bool IsCacheExpired(FileInfo file)
        {
            if (CacheExpiry == null) return false;
            return file.LastWriteTimeUtc < LocalTime.UtcNow.Subtract(CacheExpiry.Value);
        }

        /// <summary>
        /// Deletes all cached Get API results.
        /// </summary>
        public static Task DisposeCache()
        {
            lock (CacheSyncLock)
            {
                var cacheDir = GetRootCacheDirectory();
                if (cacheDir.Exists) cacheDir.Delete(true);
            }

            // Desined as a task in case in the future we need it.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes the cached Get API result for the specified API url.
        /// </summary>
        public Task DisposeCache<TResponse>(string getApiUrl)
            => GetCacheFile<TResponse>(getApiUrl).DeleteAsync(harshly: true);
    }
}