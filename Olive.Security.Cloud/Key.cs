using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Security.Cloud
{
    public class Key
    {
        public byte[] EncryptionKeyReference { get; set; }

        public byte[] EncryptionKey { get; set; }
    }
}
