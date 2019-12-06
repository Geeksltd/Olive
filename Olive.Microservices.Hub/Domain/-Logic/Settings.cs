namespace Domain
{
    using Olive;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides the business logic for Settings class.
    /// </summary>
    partial class Settings
    {
        /// <summary>
        /// Validates this instance to ensure it can be saved in a data repository.
        /// If this finds an issue, it throws a ValidationException for that.        
        /// This calls ValidateProperties(). Override this method to provide custom validation logic in a type.
        /// </summary>
        public override async Task Validate()
        {
            await base.Validate();

            if (IsNew && await Database.Any<Settings>())
                throw new Exception("Settings is Singleton!");
        }

        /// <summary>
        /// To be increment in web.config every time your JS or CSS files are changed 
        /// in order to refresh the end-users' browser cache.
        /// Also this is to be appended to the end of the resource tags' Urls.
        /// </summary>
        public static string ResourceVersion => Config.Get("App.Resource.Version");
    }
}