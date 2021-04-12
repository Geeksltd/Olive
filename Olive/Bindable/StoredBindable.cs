namespace Olive
{
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// A bindable object which is stored on disk. Its value is deserialised from disk when instantiated (as Json).
    /// Then when ever its Value is set, the json file will be updated accordingly.
    /// </summary>
    public class StoredBindable<T> : Bindable<T>
    {
        static readonly object SyncLock = new();
        static readonly JsonSerializerSettings Settings = new() { TypeNameHandling = TypeNameHandling.Auto };
        readonly FileInfo File;

        public StoredBindable(FileInfo jsonFile, T defaultValue = default)
        {
            File = jsonFile;
            Value = TryLoad(defaultValue);
            Changed += StoredBindable_Changed;
        }

        void StoredBindable_Changed()
        {
            lock (SyncLock)
                File.WriteAllText(JsonConvert.SerializeObject(Value, Settings));
        }

        T TryLoad(T defaultValue)
        {
            lock (SyncLock)
            {
                if (!File.Exists()) return defaultValue;

                try
                {
                    var text = File.ReadAllText();
                    if (text.IsEmpty()) return defaultValue;
                    return JsonConvert.DeserializeObject<T>(text, Settings) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
    }
}