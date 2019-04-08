using Olive.Entities;

namespace Olive.Mvc.CKEditorFileManager
{
    public interface ICKEditorFile : IEntity
    {
        Blob File { get; set; }
    }
}