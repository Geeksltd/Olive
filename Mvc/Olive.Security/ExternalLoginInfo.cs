namespace Olive.Security
{
    public class ExternalLoginInfo
    {
        public bool IsAuthenticated { get; set; }
        public string Issuer { get; set; }
        public string Email { get; set; }
        public string NameIdentifier { get; set; }
        public string UserName { get; set; }
    }
}
