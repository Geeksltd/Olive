using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.CKEditorFileManager
{
    public class ViewModel : IViewModel
    {
        public DownloadableFileDto[] Files { get; set; }
    }
}
