/// <amd-dependency path='app/extensions' />
export default class Service {

    private static Services: Service[] = [];

    Name: string;
    BaseUrl: string;
    AddressBarPrefix: string;
    public static PriorServiceName: string;
    public static PriorServiceUrl: string;
    public static FirstPageLoad: boolean = true;

    public GetAddressBarValueFor(fullFeatureUrl: string): string {
        let relativePath = fullFeatureUrl.trimStart(this.BaseUrl);

        if (relativePath.startsWith("/under") || relativePath.startsWith("/hub")) {
            return relativePath.trim();
        }

        return this.AddressBarPrefix.trimEnd("/") + "/" + relativePath.trimStart("/");
    }

    constructor(args: Service) {
        if (args) {
            this.BaseUrl = args.BaseUrl;
            this.Name = args.Name;
            this.AddressBarPrefix = this.Name.toLowerCase().withPrefix("/");
        }

        if (!this.BaseUrl)
            throw new Error("BaseUrl cannot be undefined");

        if (!this.Name)
            throw new Error("Name cannot be undefined");
    }

    public static registerServices(): void {
        let services = window["services"];
        if (services === undefined) return

        for (var serviceInfo of services) {
            this.Services.push(new Service(serviceInfo));
        }
    }

    public static onNavigated(fullUrl: string, windowTitle: string): void {
        let service = this.fromUrl(fullUrl);
        let currenctService = $("service").attr("of").toLowerCase();
        if (Service.PriorServiceName && Service.PriorServiceUrl && (Service.PriorServiceName.toLowerCase() != currenctService) && (Service.PriorServiceName != "hub")) {
            $(`link[href^="${Service.PriorServiceUrl}"]`).remove();
        }

        if (currenctService != Service.PriorServiceName) {
            Service.PriorServiceName = service.Name.toLowerCase();
            Service.PriorServiceUrl = service.BaseUrl;
        }

        var url = service.GetAddressBarValueFor(fullUrl);

        if (!this.FirstPageLoad)
            window.history.pushState(null, windowTitle, url);

        if (this.FirstPageLoad)
            this.FirstPageLoad = false;

        if (windowTitle)
            document.title = service.Name + ": " + windowTitle;
    }

    public static fromUrl(actualDestinationAddress: string): Service {

        for (var service of this.Services) {
            if (actualDestinationAddress.trimHttpProtocol().startsWith(service.BaseUrl.trimHttpProtocol()))
                return service;
        }

        throw new Error("Could not find a service for [" + actualDestinationAddress + "] url");
    }

    public static fromName(name: string): Service {

        name = name.toLowerCase();
        for (var service of this.Services) {
            if (name === service.Name.toLowerCase()) return service;
        }

        throw new Error("Could not find a service named '" + name + "'");
    }
}