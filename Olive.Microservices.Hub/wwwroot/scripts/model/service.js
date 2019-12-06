define(["require", "exports", "app/extensions"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    /// <amd-dependency path='app/extensions' />
    class Service {
        constructor(args) {
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
        GetAddressBarValueFor(fullFeatureUrl) {
            let relativePath = fullFeatureUrl.trimStart(this.BaseUrl);
            if (relativePath.startsWith("/under") || relativePath.startsWith("/hub")) {
                return relativePath.trim();
            }
            return this.AddressBarPrefix.trimEnd("/") + "/" + relativePath.trimStart("/");
        }
        static registerServices() {
            let services = window["services"];
            if (services === undefined)
                return;
            for (var serviceInfo of services) {
                this.Services.push(new Service(serviceInfo));
            }
        }
        static onNavigated(fullUrl, windowTitle) {
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
        static fromUrl(actualDestinationAddress) {
            for (var service of this.Services) {
                if (actualDestinationAddress.trimHttpProtocol().startsWith(service.BaseUrl.trimHttpProtocol()))
                    return service;
            }
            throw new Error("Could not find a service for [" + actualDestinationAddress + "] url");
        }
        static fromName(name) {
            name = name.toLowerCase();
            for (var service of this.Services) {
                if (name === service.Name.toLowerCase())
                    return service;
            }
            throw new Error("Could not find a service named '" + name + "'");
        }
    }
    exports.default = Service;
    Service.Services = [];
    Service.FirstPageLoad = true;
});
//# sourceMappingURL=service.js.map