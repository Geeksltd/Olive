using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    class BlobEx : Blob
    {
        public BlobEx(byte[] data, string fileName, string bindedFrom): base(data, fileName) =>
            BindedFrom = bindedFrom;

        public string BindedFrom { get; private set; }
    }
}
