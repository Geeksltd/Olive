define(["require", "exports", "app/model/service"], function (require, exports, service_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class AppContent {
        constructor(waiting, ajaxRedirect, input) {
            this.waiting = waiting;
            this.ajaxRedirect = ajaxRedirect;
            this.input = input;
        }
        enableContentBlock(selector) {
            selector.each((i, e) => new AppContent(this.waiting, this.ajaxRedirect, $(e)).enableContentBlockImpl());
        }
        enableHelp(selector) {
            selector.each((i, e) => new AppContent(this.waiting, this.ajaxRedirect, $(e)).enableHelpImpl());
        }
        enableContentBlockImpl() {
            this.checkInput();
            let keyParam = this.input.attr("key");
            this.waiting.show(true, false);
            let serviceUrl = service_1.default.fromName("AppContent");
            let url = serviceUrl.BaseUrl + "/api/getContent/" + keyParam;
            $.ajax({
                url: url,
                type: 'GET',
                xhrFields: { withCredentials: true },
                success: (response) => {
                    this.input.replaceWith(response);
                },
                error: (response) => {
                    console.log(response);
                },
                complete: (response) => {
                    this.waiting.hide();
                    this.ajaxRedirect.enableRedirect($("a[data-redirect=ajax]"));
                }
            });
        }
        enableHelpImpl() {
            this.checkInput();
            let keyParam = this.input.attr("key");
            this.waiting.show(true, false);
            let serviceUrl = service_1.default.fromName("AppContent");
            let url = serviceUrl.BaseUrl + "/api/getContent/" + keyParam + "/true";
            $.ajax({
                url: url,
                type: 'GET',
                xhrFields: { withCredentials: true },
                success: (response) => {
                    this.input.replaceWith(response);
                    $(() => {
                        $('[data-toggle="popover"]').popover({ trigger: 'focus' });
                    });
                },
                error: (response) => {
                    console.log(response);
                },
                complete: (response) => {
                    this.waiting.hide();
                    this.ajaxRedirect.enableRedirect($("a[data-redirect=ajax]"));
                }
            });
        }
        checkInput() {
            if (!this.input)
                throw new Error("The input is not provides.");
        }
    }
    exports.default = AppContent;
});
//# sourceMappingURL=appContent.js.map