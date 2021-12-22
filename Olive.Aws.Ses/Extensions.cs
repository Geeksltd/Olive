using Amazon.SimpleEmail.Model;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive.Aws.Ses
{
    internal static class Extensions
    {
        internal static RawMessage ToRawMessage(this MimeMessage message)
        {
            var stream = new MemoryStream();
            message.WriteTo(stream);
            return new RawMessage(stream);
        }
    }
}
