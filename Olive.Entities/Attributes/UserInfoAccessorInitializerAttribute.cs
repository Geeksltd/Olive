namespace Olive.Entities
{
    public class UserInfoAccessorInitializerAttribute : InitializerAttribute
    {
        public UserInfoAccessorInitializerAttribute(string initializerMethodName = "Initialize") : base(initializerMethodName) { }
    }
}
