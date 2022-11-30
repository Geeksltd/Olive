using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Aws.Ses.AutoFetch
{
    public interface IMailMessageAttachment : IEntity
    {
        Guid? MailMessageId { get; set; }        
        Blob Attachment { get; set; }
    }
}