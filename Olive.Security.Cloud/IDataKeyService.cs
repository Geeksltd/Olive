using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Security.Cloud
{
    public interface IDataKeyService
    {
        Task<Key> GenerateKey();
        byte[] GetEncryptionKey(byte[] encryptionKeyReference);
    }
}
