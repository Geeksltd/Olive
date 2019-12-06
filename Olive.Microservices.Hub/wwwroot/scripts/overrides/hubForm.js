define(["require", "exports", "olive/components/form"], function (require, exports, form_1) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class HubForm extends form_1.default {
        constructor(url, validate, waiting, ajaxRedirect) {
            super(url, validate, waiting, ajaxRedirect);
            this.currentRequestUrlProvider = () => {
                let result = window.location.pathAndQuery().trimStart("/");
                var slash = result.indexOf("/");
                if (slash > 0)
                    result = result.substring(slash);
                return result;
            };
        }
    }
    exports.default = HubForm;
});
//# sourceMappingURL=hubForm.js.map