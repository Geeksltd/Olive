using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    class MailMessage : Olive.Entities.GuidEntity, IMailMessage
    {
        public string From { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string To { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Bcc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Cc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Subject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string HtmlBody { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Sender { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Date { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
