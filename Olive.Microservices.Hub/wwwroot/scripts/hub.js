/// <amd-dependency path='olive/olivePage' />
define(["require", "exports", "olive/components/crossDomainEvent", "app/model/service", "olive/olivePage"], function (require, exports, crossDomainEvent_1, service_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class Hub {
        constructor(url, ajaxRedirect, featuresMenuFactory, breadcrumbMenu) {
            this.url = url;
            this.ajaxRedirect = ajaxRedirect;
            this.featuresMenuFactory = featuresMenuFactory;
            this.breadcrumbMenu = breadcrumbMenu;
        }
        initialize() {
            service_1.default.registerServices();
            this.featuresMenuFactory.enableFeaturesTreeView();
            this.breadcrumbMenu.enableBreadcrumb();
            window["resolveServiceUrl"] = this.url.effectiveUrlProvider;
            crossDomainEvent_1.default.handle("setViewFrameHeight", h => this.setViewFrameHeight(h));
            crossDomainEvent_1.default.handle("setServiceUrl", u => service_1.default.onNavigated(u.url, u.title));
            //initial right task menu after 3 sec delay.
            setTimeout(() => {
                $("#taskiFram").attr("src", $("#taskiFram").attr("src"));
            }, 2000);
            //this function deal with touch events for task system.
            this.initServiceWorker();
            this.loadServiceHealthChecks();
        }
        loadServiceHealthChecks() {
            console.log($(".service-tiles .tile").length);
            console.log("test");
            $(".service-tiles .tile").each((inx, item) => {
                var _this = $(item);
                _this.css("background", "yellow");
                $.get(_this.attr('url'), () => {
                    _this.css("background", "green");
                }).fail(() => {
                    _this.css("background", "red");
                });
            });
        }
        setViewFrameHeight(height) {
            if (height <= 0)
                return;
            height = Math.max($(".side-bar").height() - 400, height);
            var currentFrameHeight = $("iframe.view-frame").height();
            if (currentFrameHeight < height)
                this.setiFrameHeight(height);
            else {
                // Frame is larger. But is it too large?
                if (currentFrameHeight > height + 150)
                    this.setiFrameHeight(height);
            }
        }
        setiFrameHeight(height) {
            $("iframe.view-frame").css("cssText", "height: " + (height + 80) + "px !important;");
        }
        go(url, iframe) {
            if (iframe) {
                url = this.url.effectiveUrlProvider(url, null);
                $("iframe.view-frame").attr("src", url);
                $(".feature-frame-view").show();
                $("main").hide();
            }
            else
                this.ajaxRedirect.go(url);
        }
        initServiceWorker() {
            if ("serviceWorker" in navigator) {
                try {
                    navigator.serviceWorker
                        .register("/service-worker.js")
                        .then(() => {
                        console.log("Service worker registered");
                    })
                        .catch(error => { console.log(error); });
                }
                catch (err) {
                    console.log(err);
                }
            }
        }
    }
    exports.default = Hub;
});
//# sourceMappingURL=hub.js.map