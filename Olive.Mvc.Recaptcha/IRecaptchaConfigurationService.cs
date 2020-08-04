namespace Olive.Mvc
{
    public interface IRecaptchaConfigurationService
    {
        bool Enabled { get; }

        string JavaScriptUrl { get; }

        string ValidationMessage { get; }

        string SiteKey { get; }

        RecaptchaControlSettings ControlSettings { get; }

        string LanguageCode { get; }
    }
}
