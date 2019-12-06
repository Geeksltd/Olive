define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class ExpandCollapse {
        constructor(targetInput) {
            this.input = targetInput;
        }
        static enableExpandCollapse(selector) {
            selector.each((i, e) => new ExpandCollapse($(e)).enableExpandCollapse(e));
        }
        enableExpandCollapse(e) {
            requirejs(["js-cookie"], x => this.doEnableExpandCollapse(x, e));
        }
        doEnableExpandCollapse(Cookies, e) {
            //check cookie for the initial menu state.    
            var menuSelector = e.id === "taskBarCollapse" ? ".task-bar" : ".side-bar";
            const leftMenu = Cookies.get(menuSelector);
            if (leftMenu && leftMenu === "collapsed") {
                $(menuSelector).addClass("collapsed");
                this.changeIcon(this.input.select().children("i"));
                this.input.addClass("collapse");
            }
            else {
                this.input.removeClass("collapse");
            }
            this.input.click(() => {
                $(menuSelector).toggleClass("collapsed");
                this.input.toggleClass("collapse");
                if ($(menuSelector).hasClass("collapsed")) {
                    Cookies.set(menuSelector, "collapsed", { expires: 7 });
                }
                else {
                    Cookies.set(menuSelector, "");
                }
                this.changeIcon(this.input.select().children("i"));
                this.syncHubFrame();
            });
        }
        changeIcon(childIcon) {
            if (childIcon.hasClass("fa-chevron-left")) {
                childIcon.removeClass("fa-chevron-left").addClass("fa-chevron-right");
            }
            else {
                childIcon.removeClass("fa-chevron-right").addClass("fa-chevron-left");
            }
            this.syncHubFrame();
        }
        syncHubFrame() {
            this.notifyHub("setViewFrameHeight", Math.round($("service").height()));
        }
        notifyHub(command, arg) {
            let paramW = { command: command, arg: arg };
            window.parent.postMessage(JSON.stringify(paramW), "*");
        }
    }
    exports.default = ExpandCollapse;
});
//# sourceMappingURL=expandCollapse.js.map