using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    public interface IMailMessage : IEntity
    {
        string From { get; set; }
        string To { get; set; }
        string Bcc { get; set; }
        string Cc { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        string Sender { get; set; }
        DateTime Date { get; set; }
    }
}
