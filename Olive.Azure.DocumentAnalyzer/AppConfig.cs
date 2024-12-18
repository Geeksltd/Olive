namespace Olive;

public static class AppConfig
{
    public static string AzureDocumentIntelligenceEndpoint => Config.Get<string>("Azure:DocumentIntelligence:Endpoint");
    public static string AzureDocumentIntelligenceApiKey => Config.Get<string>("Azure:DocumentIntelligence:ApiKey");
    public static string AzureDocumentIntelligenceModelId => Config.Get<string>("Azure:DocumentIntelligence:ModelId");
    public static string APIVersion => Config.Get<string>("Azure:DocumentIntelligence:ApiVersion");
}
