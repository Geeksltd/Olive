import Waiting from 'olive/components/waiting';
declare var requirejs: any;
declare var window: any;

export default class WidgetModule {
    input: JQuery;

    constructor(targetInput: any) {
        this.input = targetInput;
    }

    public static enableWidget(selector: JQuery) {
        selector.each((i, e) => new WidgetModule($(e)).enableWidget());
    }

    enableWidget(): void {

        //hide right task menu here
         $(".task-bar").removeClass("d-lg-flex");

        this.input.append("<br/>loading...");

        $.ajax({
            url: this.input.attr("src"),
            type: 'GET',
            xhrFields: { withCredentials: true },
            success: (response) => {
                this.input.replaceWith(response);
                (<any>window.page).revive();
            },
            error: (response, x) => {
                console.log(response);
                console.log(x);
                this.input.replaceWith("<br/>Failed to load <a target='_blank' href='" + this.input.attr("src") + "'>widget</a>");
            }
        });
    }
}