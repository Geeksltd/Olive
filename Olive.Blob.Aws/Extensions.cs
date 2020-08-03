using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.BlobAws
{
    public static class Extensions
    {
        public static string GetKey(this Blob document) =>
            (document.FolderName + "/" + document.OwnerId()).KeepReplacing("//", "/").TrimStart("/");
    }
}
