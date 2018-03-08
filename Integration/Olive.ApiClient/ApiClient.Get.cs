using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive
{
    partial class ApiClient
    {
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
                var appName = AppDomain.CurrentDomain.BaseDirectory.AsDirectory().Parent.Name;
                var name = Path.Combine(Path.GetTempPath(), appName, CACHE_FOLDER, GetTypeName<TResponse>());

                return name.AsDirectory().EnsureExists().GetFile(url.ToSimplifiedSHA1Hash() + ".txt");
            }
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

            var result = default(TResponse);
            if (CachePolicy == CachePolicy.CacheOrFreshOrFail)
            {
                result = await GetCachedResponse<TResponse>();
                if (HasValue(result)) return result;
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

            if (request.Error != null && CachePolicy != CachePolicy.FreshOrFail)
                result = await GetCachedResponse<TResponse>();

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

        async Task<TResponse> DeserializeResponse<TResponse>(FileInfo file)
        {
            if (!file.Exists()) return default(TResponse);

            // TODO: Compare file.CreationTime with the specified cache time policy.
            if (CacheExpiry.HasValue && file.CreationTimeUtc < LocalTime.UtcNow.Subtract(CacheExpiry.Value))
                return default(TResponse);

            try
            {
                return JsonConvert.DeserializeObject<TResponse>(
                    await file.ReadAllTextAsync(),
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
        public Task DisposeCache()
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
        public Task DisposeCache<TResponse>(string getApiUrl)
            => GetCacheFile<TResponse>(getApiUrl).DeleteAsync(harshly: true);
    }
}