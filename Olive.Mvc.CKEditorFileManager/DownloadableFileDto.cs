using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.CKEditorFileManager
{
    public class DownloadableFileDto
    {
        public ICKEditorFile CKEditorFile { get; set; }
        public string Uri { get; set; }
    }
}
