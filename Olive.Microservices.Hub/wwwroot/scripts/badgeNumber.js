define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class BadgeNumber {
        constructor(targetInput) {
            this.input = targetInput;
        }
        static enableBadgeNumber(selector) {
            selector.each((i, e) => new BadgeNumber($(e)).enableBadgeNumber());
        }
        enableBadgeNumber() {
            let path = this.input.attr("data-badgeurl");
            if (!path)
                return;
            $.ajax({
                url: path,
                type: 'GET',
                xhrFields: { withCredentials: true },
                success: (response) => {
                    if (response > 0) {
                        const className = "data-badge";
                        if (this.input.attr("data-badge-optional") == 'true')
                            this.input.addClass("badge-optional");
                        this.input.attr(className, response);
                        //calculate parent number.
                        let feature = $(this.input.parents()[2]);
                        let child = $(feature.children("a")[0]);
                        if (child && child.attr(className)) {
                            if (this.input.attr("data-badge-optional") == "true")
                                return;
                            let total = parseInt(child.attr(className)) + parseInt(response);
                            child.attr(className, total);
                        }
                        else {
                            if (child.length > 0 && this.input.attr("data-badge-optional") == "false")
                                child.attr(className, response);
                        }
                    }
                },
                error: (response) => {
                    console.error("BadgeUrl failed: " + path);
                    console.log(response);
                }
            });
        }
    }
    exports.default = BadgeNumber;
});
//# sourceMappingURL=badgeNumber.js.map