using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.CKEditorFileManager
{
    public interface ICKEditorFile: IEntity
    {
        Blob File { get; set; }
    }
}
