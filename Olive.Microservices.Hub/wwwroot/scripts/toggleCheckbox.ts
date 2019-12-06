
export default class ToggleCheckbox {
    input: JQuery;

    constructor(targetInput: any) {
        this.input = targetInput;
    }

    public static enableToggleCheckbox(selector: JQuery) {
        selector.each((i, e) => new ToggleCheckbox($(e)).enableToggleCheckbox());
    }

    enableToggleCheckbox(): void {
        this.input.bootstrapToggle({
            on: 'Enabled',
            off: 'Disabled',
            size: "normal",
            onstyle: "primary"
        });
    }
}