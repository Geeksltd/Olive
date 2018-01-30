using System;
using System.Collections.Concurrent;
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
        static bool ShouldSuppressPersistence = Config.Get("Blob.Suppress.Persistence", defaultValue: false);
        static bool StoreWithFileName = Config.Get("Blob.Stored.With.File.Name", defaultValue: false);

        const string EMPTY_FILE = "NoFile.Empty";
        public const string DefaultEncryptionKey = "Default_ENC_Key:_This_Better_Be_Calculated_If_Possible";
        public static string SecureVirtualRoot = "/Download.File.aspx?";

        static string[] UnsafeExtensions = new[] { "aspx", "ascx", "ashx", "axd", "master", "bat", "bas", "asp", "app", "bin","cla","class", "cmd", "com","sitemap","skin", "asa", "cshtml",
            "cpl","crt","csc","dll","drv","exe","hta","htm","html", "ini", "ins","js","jse","lnk","mdb","mde","mht","mhtm","mhtml","msc", "msi","msp", "mdb", "ldb","resources", "resx",
            "mst","obj", "config","ocx","pgm","pif","scr","sct","shb","shs", "smm", "sys","url","vb","vbe","vbs","vxd","wsc","wsf","wsh" , "php", "asmx", "cs", "jsl", "asax","mdf",
            "cdx","idc", "shtm", "shtml", "stm", "browser"};

        public static ConcurrentDictionary<AccessMode, string> PhysicalFilesRoots = new ConcurrentDictionary<AccessMode, string>();

        Entity ownerEntity;
        public AccessMode FileAccessMode;
        bool IsEmptyBlob;
        byte[] FileData;

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
            FileData = data;
            this.fileName = fileName.ToSafeFileName();
        }

        /// <summary>
        /// Initializes a new Blob instance, for the specified file on disk.
        /// </summary>
		[Obsolete("By using this constructor you will async benefit, use the other ones.")]
        public Blob(FileInfo file) : this(File.ReadAllBytes(file.FullName), file.Name) { }

        /// <summary>
        /// Gets the address of the property owning this blob in the format: Type/ID/Property
        /// </summary>
        public string GetOwnerPropertyReference()
        {
            if (ownerEntity == null || OwnerProperty.IsEmpty()) return null;
            return $"{ownerEntity?.GetType().FullName}/{ownerEntity?.GetId()}/{OwnerProperty}";
        }

        public enum AccessMode { Open, Secure }

        public string OwnerProperty { get; private set; }

        string fileName, folderName;

        bool hasValue; // For performance, cache it

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
        public async Task<byte[]> GetFileDataAsync()
        {
            if (IsEmpty()) return new byte[0];

            if (FileData != null && FileData.Length > 0)
                return FileData;

            FileData = await GetStorageProvider().LoadAsync(this);

            return FileData;
        }

        public void SetData(byte[] data)
        {
            if ((data?.Length ?? 0) == 0)
                throw new InvalidOperationException("Invalid value passed.");

            FileData = data;
        }

        public string FolderName
        {
            get
            {
                if (folderName == null)
                {
                    if (ownerEntity == null) return OwnerProperty;
                    folderName = ownerEntity.GetType().Name + "." + OwnerProperty;
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
        /// Gets the Url of this blob.
        /// </summary>
        public override string ToString() => Url();

        /// <summary>
        /// Gets the content
        /// </summary>
        /// <returns></returns>
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
                throw new Exception($"The {OwnerProperty} of the {ownerEntity?.GetType().FullName} entity ({ownerEntity?.GetId()}) cannot be converted to text.", ex);
            }
        }

        /// <summary>
        /// Gets a Url to this blob.
        /// </summary>
        public string Url(AccessMode mode)
        {
            if (ownerEntity == null) return null;

            return GetVirtualFolderUrl(mode) + GetFileNameWithoutExtension() + FileExtension +
                   ("?" + FileName).OnlyWhen(mode == AccessMode.Open);
        }

        /// <summary>
        /// Gets a Url to this blob.
        /// </summary>
        public string Url() => Url(FileAccessMode);

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

        public string GetVirtualFolderUrl(AccessMode accessMode)
        {
            var root = Config.Get("UploadFolder.VirtualRoot", defaultValue: "/Documents/");
            if (accessMode == AccessMode.Secure)
                root = Config.Get("UploadFolder.VirtualRoot.Secure", defaultValue: SecureVirtualRoot);
            return root + FolderName + "/";
        }

        /// <summary>
        /// Determines whether this is an empty blob.
        /// </summary>
        public bool IsEmpty()
        {
            if (hasValue) return false;

            if (IsEmptyBlob) return true;

            if (FileName == EMPTY_FILE) return true;

            if (GetStorageProvider().CostsToCheckExistence() ||
                GetStorageProvider().FileExistsAsync(this).AwaitResult())
            {
                hasValue = true;
                return false;
            }

            if (FileData == null) return true;

            return FileData.None();
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

            if (ownerEntity != null)
            {
                result = new Blob(await GetFileDataAsync(), FileName);
                if (attach)
                {
                    if (!@readonly) Attach(ownerEntity, OwnerProperty, FileAccessMode);
                    else
                    {
                        result.ownerEntity = ownerEntity;
                        result.OwnerProperty = OwnerProperty;
                        result.FileAccessMode = FileAccessMode;
                    }
                }
            }
            else
            {
                if (FileData != null && FileData.Any()) result = new Blob(FileData, FileName);
                else result = new Blob(FileName);
            }

            return result;
        }

        /// <summary>
        /// Attaches this Blob to a specific record's file property.
        /// </summary>
        public Blob Attach(Entity owner, string propertyName) => Attach(owner, propertyName, AccessMode.Open);

        /// <summary>
        /// Attaches this Blob to a specific record's file property.
        /// </summary>
        public Blob Attach(Entity owner, string propertyName, AccessMode accessMode)
        {
            ownerEntity = owner;
            OwnerProperty = propertyName;
            FileAccessMode = accessMode;

            if (owner is GuidEntity) owner.Saving.Handle(Owner_Saving);
            else owner.Saved.Handle(Owner_Saved);

            owner.Deleting.Handle(Delete);
            return this;
        }

        /// <summary>
        /// Detaches this Blob.
        /// </summary>
        public void Detach()
        {
            if (ownerEntity == null) return;

            ownerEntity.Saving.RemoveHandler(Owner_Saving);
            ownerEntity.Saved.RemoveHandler(Owner_Saved);
            ownerEntity.Deleting.RemoveHandler(Delete);
        }

        // TODO: Deleting should be async and so on.

        /// <summary>
        /// Deletes this blob from the disk.
        /// </summary>
        Task Delete(EventArgs e)
        {
            if (ShouldSuppressPersistence) return Task.CompletedTask;

            if (ownerEntity.GetType().Defines<SoftDeleteAttribute>()) return Task.CompletedTask;

            DeleteFromDisk();

            return Task.CompletedTask;
        }

        void DeleteFromDisk()
        {
            if (ownerEntity == null) throw new InvalidOperationException();

            GetStorageProvider().DeleteAsync(this);

            FileData = null;
        }

        async Task Owner_Saving(System.ComponentModel.CancelEventArgs e)
        {
            if (!ShouldSuppressPersistence) await SaveOnDisk();
        }

        async Task Owner_Saved(SaveEventArgs e)
        {
            if (!ShouldSuppressPersistence) await SaveOnDisk();
        }

        /// <summary>
        /// Saves this file on the disk.
        /// </summary>
        public async Task SaveOnDisk()
        {
            if (FileData != null && FileData.Length > 0)
                await GetStorageProvider().SaveAsync(this);

            else if (IsEmptyBlob) DeleteFromDisk();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public string GetMimeType() => $"c:\\{FileName}".AsFile().GetMimeType();// The blob may be in-memory.

        /// <summary>Determines if this blob's file extension is for audio or video.</summary>
        public bool IsMedia() => GetMimeType().StartsWithAny("audio/", "video/");

        string GetFilesRoot() => GetPhysicalFilesRoot(FileAccessMode).FullName;

        /// <summary>
        /// Gets the physical path root.
        /// </summary>
        public static DirectoryInfo GetPhysicalFilesRoot(AccessMode accessMode)
        {
            var result = PhysicalFilesRoots.GetOrAdd(accessMode,
                m =>
                {
                    var folderConfigKey = accessMode == AccessMode.Secure ? "UploadFolder.Secure" : "UploadFolder";
                    var defaultFolder = accessMode == AccessMode.Secure ? "App_Data\\" : "Documents\\";

                    var folder = Config.Get(folderConfigKey).Or(defaultFolder).TrimEnd('\\') + "\\";

                    if (!folder.StartsWith("\\\\") && folder[1] != ':') // Relative address:
                        folder = AppDomain.CurrentDomain.WebsiteRoot().GetSubDirectory(folder).FullName;

                    return folder;
                });

            return new DirectoryInfo(result);
        }

        internal string LocalFolder
        {
            get
            {
                if (ownerEntity == null) return null;

                var docsFolder = Path.Combine(GetFilesRoot(), FolderName + "\\");

                return docsFolder;
            }
        }

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

            var entity = await Entity.Database.GetOrDefault(id, type);
            if (entity == null)
                throw new ArgumentException($"Could not load an instance of '{parts.First()}' with the ID of '{id} from the database.");

            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new Exception($"The type {type.FullName} does not have a property named {propertyName}.");

            return property.GetValue(entity) as Blob;
        }

        /// <summary>
        /// Gets the local physical path of this file.
        /// </summary>
        public string LocalPath
        {
            get
            {
                if (ownerEntity == null) return null;

                var result = Path.Combine(LocalFolder, GetFileNameWithoutExtension() + FileExtension);

                if (!Directory.Exists(LocalFolder)) Directory.CreateDirectory(LocalFolder);

                return result;
            }
        }

        public string GetFileNameWithoutExtension()
        {
            if (ownerEntity == null) return null;
            if (ownerEntity is IntEntity && ownerEntity.IsNew) return null;

            if (StoreWithFileName)
                return FileName.TrimEnd(FileExtension);

            return ownerEntity?.GetId().ToStringOrEmpty();
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

            var extension = Path.GetExtension(fileName).OrEmpty().Where(x => x.IsLetter()).ToArray().ToString("").ToLower();

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
                var me = FileData?.Length;
                var him = other.FileData?.Length;
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