using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary> 
    /// Provides an utility for handling Binary property types.
    /// </summary>
    public class Blob : IComparable<Blob>, IComparable
    {
        /// <summary>
        /// In Test projects particularly, having files save themselves on the disk can waste space.
        /// To prevent that, apply this setting in the config file.
        /// </summary>
        static bool SuppressPersistence = Config.Get("Blob:WebTest:SuppressPersistence", defaultValue: false);

        public const string EMPTY_FILE = "NoFile.Empty";
        public const string UNCHANGED_FILE = "«UNCHANGED»";
        public const string DefaultEncryptionKey = "Default_ENC_Key:_This_Better_Be_Calculated_If_Possible";

        static string[] UnsafeExtensions = new[] { "aspx", "ascx", "ashx", "axd", "master", "bat", "bas", "asp", "app", "bin","cla","class", "cmd", "com","sitemap","skin", "asa", "cshtml",
            "cpl","crt","csc","dll","drv","exe","hta","htm","html", "ini", "ins","js","jse","lnk","mdb","mde","mht","mhtm","mhtml","msc", "msi","msp", "mdb", "ldb","resources", "resx",
            "mst","obj", "config","ocx","pgm","pif","scr","sct","shb","shs", "smm", "sys","url","vb","vbe","vbs","vxd","wsc","wsf","wsh" , "php", "asmx", "cs", "jsl", "asax","mdf",
            "cdx","idc", "shtm", "shtml", "stm", "browser"};

        internal Entity OwnerEntity;
        bool IsEmptyBlob;
        protected byte[] CachedFileData, NewFileData;
        string fileName, folderName;
        bool hasValue; // For performance, cache it

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        public Blob() { }

        /// <summary>
        /// Initializes a new Document instance with the specified file name.
        /// </summary>
        public Blob(string fileName) : this(null, fileName) { }

        /// <summary>
        /// Initializes a new Blob instance with the specified data and file name.
        /// </summary>
        public Blob(byte[] data, string fileName)
        {
            NewFileData = data;
            this.fileName = fileName.ToSafeFileName();
        }

        /// <summary>
        /// Initializes a new Blob instance, for the specified file on disk.
        /// </summary>
		[Obsolete("By using this constructor you will async benefit, use the other ones.")]
        public Blob(FileInfo file) : this(File.ReadAllBytes(file.FullName), file.Name) { }

        public string OwnerProperty { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [EscapeGCop("This is to defined as extension method handle null as well.")]
        public static bool HasFileDataInMemory(Blob blob) => blob?.NewFileData?.Length > 0;

        static IDatabase Database => Context.Current.Database();

        public string FileName
        {
            get { return fileName.Or(EMPTY_FILE); }
            set { fileName = value; }
        }

        public string FileExtension
        {
            get
            {
                if (fileName.IsEmpty()) return string.Empty;
                else
                {
                    var result = Path.GetExtension(fileName) ?? string.Empty;
                    if (result.Length > 0 && !result.StartsWith("."))
                        result = "." + result;
                    return result;
                }
            }
        }

        /// <summary>
        /// Gets the data of this blob.
        /// </summary>
        public virtual async Task<byte[]> GetFileDataAsync()
        {
            if (IsEmpty()) return new byte[0];

            if (NewFileData != null && NewFileData.Length > 0)
                return NewFileData;

            return CachedFileData = await GetStorageProvider().LoadAsync(this);
        }

        public void SetData(byte[] data)
        {
            if ((data?.Length ?? 0) == 0)
                throw new InvalidOperationException("Invalid value passed.");

            NewFileData = data;
        }

        public string FolderName
        {
            get
            {
                if (folderName is null)
                {
                    if (OwnerEntity == null) return OwnerProperty;
                    folderName = OwnerEntity.GetType().Name + "." + OwnerProperty;
                }

                return folderName;
            }
            set => folderName = value;
        }

        IBlobStorageProvider GetStorageProvider() => BlobStorageProviderFactory.GetProvider(FolderName);

        /// <summary>
        /// Gets an empty blob object.
        /// </summary>
        public static Blob Empty() => new Blob(null, EMPTY_FILE) { IsEmptyBlob = true };

        /// <summary>
        /// Gets an empty blob object.
        /// </summary>
        public static Blob Unchanged() => new Blob(new byte[0], UNCHANGED_FILE);

        /// <summary>
        /// Gets the Url of this blob.
        /// </summary>
        public override string ToString() => Url();

        /// <summary>
        /// Gets the content
        /// </summary>
        public async Task<string> GetContentTextAsync()
        {
            if (IsEmpty()) return string.Empty;

            try
            {
                using (var mem = new MemoryStream(await GetFileDataAsync()))
                {
                    using (var reader = new StreamReader(mem))
                        return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The {OwnerProperty} of the {OwnerEntity?.GetType().FullName} entity ({OwnerEntity?.GetId()}) cannot be converted to text.", ex);
            }
        }

        /// <summary>
        /// Gets a Url to this blob.
        /// </summary>
        public string Url()
        {
            if (OwnerEntity == null) return null;
            var result = Config.Get("Blob:BaseUrl") + FolderName + "/" + OwnerId();
            if (Config.Get("Blob:UrlWithExtension", defaultValue: true)) result += FileExtension;
            return result;
        }

        /// <summary>
        /// Returns the Url of this blob, or the provided default Url if this is Empty.
        /// </summary>
        public string UrlOr(string defaultUrl)
        {
            if (IsEmpty()) return defaultUrl;
            else return Url();
        }

        /// <summary>
        /// Gets a cache safe URL to this blob.
        /// </summary>
        public string GetCacheSafeUrl()
        {
            var result = Url();

            if (result.IsEmpty()) return result;

            return result + (result.Contains("?") ? "&" : "?") + "RANDOM=" + Guid.NewGuid();
        }

        public bool IsUnchanged() => FileName == UNCHANGED_FILE;

        /// <summary>
        /// Determines whether this is an empty blob.
        /// </summary>
        public bool IsEmpty()
        {
            if (hasValue) return false;

            if (IsEmptyBlob) return true;

            if (FileName == EMPTY_FILE) return true;

            if (NewFileData.HasAny()) return false;

            if (GetStorageProvider().CostsToCheckExistence())
            {
                // We don't want to incur cost.
                // As the file name has value, we assume the file does exist.
                hasValue = true;
                return false;
            }
            else if (OwnerEntity is null)
            {
                throw new InvalidOperationException("This blob is not attached to an entity.");
            }
            else if (Task.Factory.RunSync(() => GetStorageProvider().FileExistsAsync(this)))
            {
                hasValue = true;
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Determines whether this blob has any content.
        /// </summary>
        public bool HasValue() => !IsEmpty();

        /// <summary>
        /// Creates a clone of this blob.
        /// </summary>
        public Task<Blob> CloneAsync() => CloneAsync(attach: false, @readonly: false);

        /// <summary>
        /// Creates a clone of this blob.
        /// </summary>
        public Blob Clone() => Task.Factory.RunSync(() => CloneAsync(attach: false, @readonly: false));

        public async Task<Blob> CloneAsync(bool attach, bool @readonly)
        {
            if (!attach && @readonly) throw new ArgumentException("readonly can be set to true only when attaching.");

            Blob result;

            if (OwnerEntity != null)
            {
                if (NewFileData.HasAny())
                    result = new Blob(await GetFileDataAsync(), FileName);
                else result = new ClonedDocument(this);
                if (attach)
                {
                    if (!@readonly) Attach(OwnerEntity, OwnerProperty);
                    else
                    {
                        result.OwnerEntity = OwnerEntity;
                        result.OwnerProperty = OwnerProperty;
                    }
                }
            }
            else
            {
                if (NewFileData.None()) result = new Blob(FileName);
                else result = new Blob(NewFileData, FileName);
            }

            return result;
        }

        /// <summary>
        /// Attaches this Blob to a specific record's file property.
        /// </summary>
        public Blob Attach(Entity owner, string propertyName)
        {
            OwnerEntity = owner;
            OwnerProperty = propertyName;
            if (owner is GuidEntity) owner.Saving += Owner_Saving;
            else owner.Saved += Owner_Saved;

            owner.Deleting += Delete;
            return this;
        }

        /// <summary>
        /// Detaches this Blob.
        /// </summary>
        public void Detach()
        {
            if (OwnerEntity == null) return;

            OwnerEntity.Saving -= Owner_Saving;
            OwnerEntity.Saved -= Owner_Saved;
            OwnerEntity.Deleting -= Delete;
        }

        // TODO: Deleting should be async and so on.

        /// <summary>Deletes this blob from the storage provider.</summary>
        void Delete(AwaitableEvent<CancelEventArgs> ev)
        {
            if (SuppressPersistence) return;
            if (OwnerEntity.GetType().Defines<SoftDeleteAttribute>()) return;
            ev.Do(DeleteAsync);
        }

        async Task DeleteAsync()
        {
            if (OwnerEntity == null) throw new InvalidOperationException();
            await GetStorageProvider().DeleteAsync(this);
            CachedFileData = NewFileData = null;
        }

        void Owner_Saving(AwaitableEvent<CancelEventArgs> ev)
        {
            if (!SuppressPersistence) ev.Do(Save);
        }

        void Owner_Saved(AwaitableEvent<SaveEventArgs> ev)
        {
            if (!SuppressPersistence) ev.Do(Save);
        }

        /// <summary>Saves this file to the storage provider.</summary>
        public virtual async Task Save()
        {
            if (NewFileData.HasAny())
                await GetStorageProvider().SaveAsync(this);
            else if (IsEmptyBlob) await DeleteAsync();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public string GetMimeType() => $"c:\\{FileName}".AsFile().GetMimeType();// The blob may be in-memory.

        /// <summary>Determines if this blob's file extension is for audio or video.</summary>
        public bool IsMedia() => GetMimeType().StartsWithAny("audio/", "video/");

        /// <summary>
        ///  This will return the blob object linked to the correct entity.
        /// </summary>
        /// <param name="reference">Expected format: Type/Id/Property.</param>
        public static async Task<Blob> FromReference(string reference)
        {
            var parts = reference.OrEmpty().Split('/');
            if (parts.Length != 3) throw new ArgumentException("Expected format is Type/ID/Property.");

            var type = EntityFinder.GetEntityType(parts.First());

            if (type == null)
                throw new ArgumentException($"The type '{parts.First()}' is not found in the currently loaded assemblies.");

            var id = parts[1];
            var propertyName = parts.Last();

            var entity = await Database.GetOrDefault(id, type);
            if (entity == null)
                throw new ArgumentException($"Could not load an instance of '{parts.First()}' with the ID of '{id} from the database.");

            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new Exception($"The type {type.FullName} does not have a property named {propertyName}.");

            return property.GetValue(entity) as Blob;
        }

        public string OwnerId()
        {
            if (OwnerEntity == null) return null;
            if (OwnerEntity is IntEntity && OwnerEntity.IsNew) return null;

            return OwnerEntity?.GetId().ToStringOrEmpty();
        }

        #region Unsafe Files Handling

        /// <summary>
        /// Gets a list of unsafe file extensions.
        /// </summary>
        public static string[] GetUnsafeExtensions() => UnsafeExtensions;

        /// <summary>
        /// Determines whether the extension of this file is potentially unsafe.
        /// </summary>
        public bool HasUnsafeExtension() => HasUnsafeFileExtension(FileName);

        public static bool HasUnsafeFileExtension(string fileName)
        {
            if (fileName.IsEmpty()) return false;

            var extension = Path.GetExtension(fileName.Trim().TrimEnd('.', '\\', '/'))
                .OrEmpty().Where(x => x.IsLetter()).ToArray().ToString("").ToLower();

            return UnsafeExtensions.Contains(extension);
        }

        #endregion

        public override bool Equals(object obj)
        {
            var other = obj as Blob;

            if (other == null) return false;
            else if (ReferenceEquals(this, other)) return true;
            else if (IsEmpty() && other.IsEmpty()) return true;

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(Blob left, Blob right)
        {
            if (ReferenceEquals(left, right)) return true;

            else if (left is null) return false;

            else return left.Equals(right);
        }

        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);

        public static bool operator !=(Blob left, Blob right) => !(left == right);

        /// <summary>
        /// Gets this blob if it has a value, otherwise another specified blob.
        /// </summary>
        public Blob Or(Blob other)
        {
            if (IsEmpty()) return other;
            else return this;
        }

        /// <summary>
        /// Compares this blob versus a specified other blob.
        /// </summary>
        public int CompareTo(Blob other)
        {
            if (other == null) return 1;

            if (IsEmpty()) return other.IsEmpty() ? 0 : -1;

            if (other.IsEmpty()) return 1;
            else
            {
                var me = NewFileData?.Length;
                var him = other.NewFileData?.Length;
                if (me == him) return 0;
                if (me > him) return 1;
                else return -1;
            }
        }

        /// <summary>
        /// Compares this blob versus a specified other blob.
        /// </summary>
        public int CompareTo(object obj) => CompareTo(obj as Blob);
    }
}