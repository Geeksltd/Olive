namespace Olive.Email
{
    public class EmailConfiguration
    {
        public bool EnableSsl { get; set; }
        public int SmtpPort { get; set; }
        public int MaxRetries { get; set; } = 4;

        public string AutoAddCc { get; set; }
        public string AutoAddBcc { get; set; }

        public string SmtpHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public MailAddress From { get; set; }
        public MailAddress ReplyTo { get; set; }

        public WhiteListSettings Permitted { get; set; }

        public class MailAddress
        {
            public string Address { get; set; }
            public string Name { get; set; }
        }

        public class WhiteListSettings
        {
            public string Domains { get; set; }
            public string Addresses { get; set; }
        }
    }
}