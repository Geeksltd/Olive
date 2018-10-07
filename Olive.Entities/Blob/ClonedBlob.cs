using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary>
    /// Created from a persisted Blob to prevent unnecessary file loading when it's not actually changed.
    /// So that if an entity is being updated, while its original file is not changed, we don't do an unnecessary file operation.
    /// </summary>
    class ClonedDocument : Blob
    {
        Blob Original;

        public ClonedDocument(Blob original) : base(original.FileName) => Original = original;

        bool BelongsToOriginal => OwnerEntity == Original.OwnerEntity && OwnerProperty == Original.OwnerProperty;

        public override Task<byte[]> GetFileDataAsync() => Original.GetFileDataAsync();

        public override Task Save()
        {
            if (BelongsToOriginal) return Task.CompletedTask;
            else return base.Save();
        }
    }
}
