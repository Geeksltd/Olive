define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class WidgetModule {
        constructor(targetInput) {
            this.input = targetInput;
        }
        static enableWidget(selector) {
            selector.each((i, e) => new WidgetModule($(e)).enableWidget());
        }
        enableWidget() {
            //hide right task menu here
            $(".task-bar").removeClass("d-lg-flex");
            this.input.append("<br/>loading...");
            $.ajax({
                url: this.input.attr("src"),
                type: 'GET',
                xhrFields: { withCredentials: true },
                success: (response) => {
                    this.input.replaceWith(response);
                    window.page.revive();
                },
                error: (response, x) => {
                    console.log(response);
                    console.log(x);
                    this.input.replaceWith("<br/>Failed to load <a target='_blank' href='" + this.input.attr("src") + "'>widget</a>");
                }
            });
        }
    }
    exports.default = WidgetModule;
});
//# sourceMappingURL=widgetModule.js.map