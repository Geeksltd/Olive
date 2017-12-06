namespace Olive.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InitializerAttribute : Attribute
    {
        public string InitializerMethodName { get; }

        public InitializerAttribute(string initializerMethodName = "Initialize") =>
            InitializerMethodName = initializerMethodName;

        public static void InvokeInitializeMethod<T>(Assembly assembly) where T : InitializerAttribute
        {
            var initializers = assembly.SelectTypesByAttribute<T>(inherit: false);

            if (initializers.Count() == 1)
            {
                var initializer = initializers.First();
                var initializerAttribute = initializer.GetCustomAttribute<T>(inherit: false);

                (initializer.GetMethod(initializerAttribute.InitializerMethodName) ??
                    throw new Exception($"The initailizer class does not have the {initializerAttribute.InitializerMethodName} method or it is not a static method.")
                    ).Invoke(null, null);
            }

            else if (initializers.Count() == 0)
                throw new Exception("The given assembly has no initializer.");
        }
    }
}