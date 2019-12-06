define(["require", "exports", "olive/mvc/ajaxRedirect", "app/model/service", "app/error/errorViewsNavigator"], function (require, exports, ajaxRedirect_1, service_1, errorViewsNavigator_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class HubAjaxRedirect extends ajaxRedirect_1.default {
        constructor(url, responseProcessor, waiting) {
            super(url, responseProcessor, waiting);
        }
        onRedirected(title, url) {
            service_1.default.onNavigated(url, title);
        }
        onRedirectionFailed(url, response) {
            let service = service_1.default.fromUrl(url);
            if (service)
                errorViewsNavigator_1.default.goToServiceError(service, url);
            else
                super.onRedirectionFailed(url, response);
        }
    }
    exports.default = HubAjaxRedirect;
});
//# sourceMappingURL=hubAjaxRedirect.js.map