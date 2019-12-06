define(["require", "exports", "olive/components/url", "app/model/service"], function (require, exports, url_1, service_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class HubUrl extends url_1.default {
        constructor() {
            super(...arguments);
            this.effectiveUrlProvider = (url, trigger) => {
                $("#iFrameHolder").hide(); //hide any opened iFrame content after ajax call.
                let serviceName;
                let serviceContainer = trigger ? trigger.closest("service[of]") : $("service[of]");
                if (serviceContainer.length === 0)
                    serviceContainer = $("service[of]");
                if (serviceContainer.length === 0)
                    throw new Error("<service of='...' /> is not found on the page.");
                serviceName = serviceContainer.attr("of").toLocaleLowerCase();
                if (!this.isAbsolute(url)) {
                    let innerUrl = "";
                    if (url.startsWith("/"))
                        innerUrl = url.trimStart("/");
                    else
                        innerUrl = url;
                    // Explicitly specified on the link?
                    if (innerUrl.startsWith("[") && innerUrl.contains("]")) {
                        serviceName = innerUrl.substring(1, innerUrl.indexOf("]"));
                        innerUrl = innerUrl.substring(serviceName.length + 2);
                        serviceContainer.attr("of", serviceName);
                    }
                    //All urls starting with "under" are from HUB service.
                    if (innerUrl.startsWith("under"))
                        serviceName = "hub";
                    //hide or show right task menu
                    (serviceName === "tasks") ? $(".task-bar").removeClass("d-lg-flex") : $(".task-bar").addClass("d-lg-flex");
                    var baseUrl = service_1.default.fromName(serviceName).BaseUrl;
                    if (!baseUrl.startsWith("http"))
                        baseUrl = baseUrl.withPrefix("http://");
                    return this.makeAbsolute(baseUrl, innerUrl);
                }
                if (url.contains("/under/")) {
                    serviceContainer.attr("of", "Hub");
                    //hide or show right task menu
                    if (!$(".task-bar").hasClass("d-lg-flex"))
                        $(".task-bar").addClass("d-lg-flex");
                }
                //URL is absolute for sure
                return url;
            };
        }
    }
    exports.default = HubUrl;
});
//# sourceMappingURL=hubUrl.js.map