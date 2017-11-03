namespace Olive.Services.Email
{
    internal class SmtpNetworkSetting
    {
        public bool DefaultCredentials { get; set; }
        public string Host { get; set; }
        public string TargetName { get; set; }
        public string ClientDomain { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public bool EnableSsl { get; set; }
    }
}