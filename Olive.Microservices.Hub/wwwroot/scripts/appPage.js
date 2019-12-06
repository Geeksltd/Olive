define(["require", "exports", "olive/olivePage", "app/featuresMenu/featuresMenu", "app/appContent", "app/badgeNumber", "app/toggleCheckbox", "app/widgetModule", "app/expandCollapse", "app/featuresMenu/breadcrumbMenu", "olive/di/services", "./overrides/hubAjaxRedirect", "./overrides/hubForm", "./hubServices", "./hub", "./overrides/hubUrl"], function (require, exports, olivePage_1, featuresMenu_1, appContent_1, badgeNumber_1, toggleCheckbox_1, widgetModule_1, expandCollapse_1, breadcrumbMenu_1, services_1, hubAjaxRedirect_1, hubForm_1, hubServices_1, hub_1, hubUrl_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class AppPage extends olivePage_1.default {
        // Here you can override any of the base standard functions.
        // e.g: To use a different AutoComplete library, simply override handleAutoComplete(input).
        constructor() {
            super();
            this.getService(hubServices_1.default.Hub).initialize();
            badgeNumber_1.default.enableBadgeNumber($("a[data-badgeurl]"));
            //every 5 min badge numbers should be updated
            window.setInterval(() => { badgeNumber_1.default.enableBadgeNumber($("a[data-badgeurl]")); }, 5 * 60 * 1000);
            expandCollapse_1.default.enableExpandCollapse($("#sidebarCollapse"));
            expandCollapse_1.default.enableExpandCollapse($("#taskBarCollapse"));
            $(() => {
                // set global search focused
                $("input.global-search").each((i, e) => e.focus());
            });
            $("#iFrameHolder").hide(); //hide iFrame initially
        }
        configureServices(services) {
            services.addSingleton(services_1.default.Url, () => new hubUrl_1.default());
            services.addSingleton(hubServices_1.default.Hub, (url, ajaxRedirect, featuresMenuFactory, breadcrumbMenu) => new hub_1.default(url, ajaxRedirect, featuresMenuFactory, breadcrumbMenu))
                .withDependencies(services_1.default.Url, services_1.default.AjaxRedirect, hubServices_1.default.FeaturesMenuFactory, hubServices_1.default.BreadcrumbMenu);
            services.addSingleton(hubServices_1.default.FeaturesMenuFactory, (url, waiting, ajaxRedirect) => new featuresMenu_1.FeaturesMenuFactory(url, waiting, ajaxRedirect))
                .withDependencies(services_1.default.Url, services_1.default.Waiting, services_1.default.AjaxRedirect);
            services.addSingleton(hubServices_1.default.AppContent, (waiting, ajaxRedirect) => new appContent_1.default(waiting, ajaxRedirect))
                .withDependencies(services_1.default.Waiting, services_1.default.AjaxRedirect);
            services.addSingleton(hubServices_1.default.BreadcrumbMenu, (ajaxRedirect) => new breadcrumbMenu_1.default(ajaxRedirect))
                .withDependencies(services_1.default.AjaxRedirect);
            services.addSingleton(services_1.default.AjaxRedirect, (url, responseProcessor, waiting) => new hubAjaxRedirect_1.default(url, responseProcessor, waiting))
                .withDependencies(services_1.default.Url, services_1.default.ResponseProcessor, services_1.default.Waiting);
            services.addSingleton(services_1.default.Form, (url, validate, waiting, ajaxRedirect) => new hubForm_1.default(url, validate, waiting, ajaxRedirect))
                .withDependencies(services_1.default.Url, services_1.default.Validate, services_1.default.Waiting, services_1.default.AjaxRedirect);
            super.configureServices(services);
        }
        revive() {
            super.initialize();
        }
        initialize() {
            super.initialize();
            this.getService(hubServices_1.default.FeaturesMenuFactory).bindItemListClick();
            this.getService(hubServices_1.default.BreadcrumbMenu).bindItemListClick();
            const appcontext = this.getService(hubServices_1.default.AppContent);
            appcontext.enableContentBlock($("AppContent"));
            appcontext.enableHelp($("Help"));
            toggleCheckbox_1.default.enableToggleCheckbox($("input[class='form-check']"));
            widgetModule_1.default.enableWidget($("Widget"));
            let currentService = $("service[of]").attr("of");
            if (currentService) {
                currentService = currentService.toLocaleLowerCase();
                if (currentService === "tasks") {
                    $('#taskiFram').attr('src', $('#taskiFram').attr('src'));
                }
            }
            // This function is called upon every Ajax update as well as the initial page load.
            // Any custom initiation goes here.
        }
    }
    exports.default = AppPage;
    window["page"] = new AppPage();
});
//# sourceMappingURL=appPage.js.map