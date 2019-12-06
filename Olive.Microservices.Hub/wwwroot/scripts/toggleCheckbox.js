define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class ToggleCheckbox {
        constructor(targetInput) {
            this.input = targetInput;
        }
        static enableToggleCheckbox(selector) {
            selector.each((i, e) => new ToggleCheckbox($(e)).enableToggleCheckbox());
        }
        enableToggleCheckbox() {
            this.input.bootstrapToggle({
                on: 'Enabled',
                off: 'Disabled',
                size: "normal",
                onstyle: "primary"
            });
        }
    }
    exports.default = ToggleCheckbox;
});
//# sourceMappingURL=toggleCheckbox.js.map