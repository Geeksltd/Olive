using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    public class ApiResponseCache<TData> : ApiResponseCache
    {
        static ConcurrentDictionary<string, TData> DeserializedCache = new ConcurrentDictionary<string, TData>();

        public TData Data { get; internal set; }

        internal async Task<bool> HasValidValue(TimeSpan? expiry = null)
        {
            if (!File.Exists()) return false;
            if (expiry.HasValue && Age > expiry) return false;

            if (DeserializedCache.TryGetValue(Key, out var data)) Data = data;
            else await LoadData();

            if (ReferenceEquals(Data, null)) return false;
            if (Data.Equals(default(TData))) return false;

            return true;
        }

        string Key => File.FullName + "|" + typeof(TData).FullName;

        async Task LoadData()
        {
            if (!File.Exists()) return;

            try
            {
                var text = await File.ReadAllTextAsync();
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

                Data = JsonConvert.DeserializeObject<TData>(text, settings);
                DeserializedCache.TryAdd(Key, Data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to deserialize the cached file into " +
                    typeof(TData).Name + " : " + ex.Message);
            }
        }

        internal static ApiResponseCache<TData> Create(string url)
        {
            FileInfo file;

            lock (CacheSyncLock)
                file = GetCacheDirectory().GetFile(url.ToSimplifiedSHA1Hash() + ".txt");

            return new ApiResponseCache<TData>()
            {
                Url = url,
                File = file,
                CreationDate = file.LastWriteTimeUtc
            };
        }

        static DirectoryInfo GetCacheDirectory()
        {
            return GetRootCacheDirectory().GetOrCreateSubDirectory(GetTypeName<TData>());
        }
    }

    public abstract class ApiResponseCache
    {
        const string CACHE_FOLDER = "-ApiCache";
        protected static object CacheSyncLock = new object();

        protected static DirectoryInfo GetRootCacheDirectory()
        {
            var appName = AppDomain.CurrentDomain.BaseDirectory.AsDirectory().Parent.Name;
            return Path.Combine(Path.GetTempPath(), appName, CACHE_FOLDER).AsDirectory().EnsureExists();
        }

        public string Message { get; internal set; }

        public string Url { get; internal set; }

        public string OriginalErrorMessage { get; internal set; }

        public FileInfo File { get; internal set; }

        public DateTime CreationDate { get; internal set; }

        public TimeSpan Age => LocalTime.Now.Subtract(CreationDate);

        protected static string GetTypeName<T>()
           => typeof(T).GetGenericArguments().SingleOrDefault()?.Name ?? typeof(T).Name.Replace("[]", "");

        static string GetTypeName<T>(T modified) => modified.GetType().Name;

        internal static Task DisposeAll()
        {
            lock (CacheSyncLock)
            {
                var cacheDir = GetRootCacheDirectory();
                if (cacheDir.Exists) cacheDir.Delete(recursive: true);
            }

            // Desined as a task in case in the future we need it.
            return Task.CompletedTask;
        }
    }
}