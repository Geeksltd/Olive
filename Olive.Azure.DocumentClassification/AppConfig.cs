namespace Olive
{
    public static class AppConfig
    {
        public static string AzureDocumentIntelligenceEndpoint => Config.Get<string>("Azure:Classification:Endpoint");
        public static string AzureDocumentIntelligenceApiKey => Config.Get<string>("Azure:Classification:ApiKey");
        public static string AzureDocumentIntelligenceModelId => Config.Get<string>("Azure:Classification:ModelId");
        public static string APIVersion => Config.Get<string>("Azure:Classification:ApiVersion");
    }
}
