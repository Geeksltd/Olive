import Waiting from "olive/components/waiting";
import Service from "app/model/service";
import AjaxRedirect from "olive/mvc/ajaxRedirect";

export default class AppContent implements IService{
    public enableContentBlock(selector: JQuery) {
        selector.each((i, e) => new AppContent(this.waiting, this.ajaxRedirect, $(e)).enableContentBlockImpl());
    }

    public enableHelp(selector: JQuery) {
        selector.each((i, e) => new AppContent(this.waiting, this.ajaxRedirect, $(e)).enableHelpImpl());
    }

    constructor(private waiting: Waiting, private ajaxRedirect: AjaxRedirect, private input?: JQuery) { }

    private enableContentBlockImpl(): void {
        this.checkInput();

        let keyParam = this.input.attr("key");

        this.waiting.show(true, false);

        let serviceUrl = Service.fromName("AppContent");

        let url = serviceUrl.BaseUrl + "/api/getContent/" + keyParam;

        $.ajax({
            url: url,
            type: 'GET',
            xhrFields: { withCredentials: true },
            success: (response: any) => {
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

    private enableHelpImpl(): void {
        this.checkInput();

        let keyParam = this.input.attr("key");

        this.waiting.show(true, false);

        let serviceUrl = Service.fromName("AppContent");

        let url = serviceUrl.BaseUrl + "/api/getContent/" + keyParam + "/true";

        $.ajax({
            url: url,
            type: 'GET',
            xhrFields: { withCredentials: true },
            success: (response: any) => {

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

    private checkInput() {
        if (!this.input)
            throw new Error("The input is not provides.");
    }
}
