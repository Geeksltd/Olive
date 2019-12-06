
export default class BadgeNumber {
    input: JQuery;

    constructor(targetInput: any) {
        this.input = targetInput;
    }

    public static enableBadgeNumber(selector: JQuery) {
        selector.each((i, e) => new BadgeNumber($(e)).enableBadgeNumber());
    }

    enableBadgeNumber(): void {
        let path = this.input.attr("data-badgeurl");
        if (!path) return;

        $.ajax({
            url: path,
            type: 'GET',
            xhrFields: { withCredentials: true },
            success: (response: any) => {

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