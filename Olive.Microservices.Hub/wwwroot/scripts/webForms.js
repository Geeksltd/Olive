$(function () {
    if (window.location.href.indexOf("/widget/") > 0)
        return;
    syncHubFrame();
    var form = $("form");
    form.resize(() => syncHubFrame());
    $(window).resize(() => syncHubFrame());
    $(window).on('load', () => syncHubFrame());
    window.addEventListener('beforeunload', function () { window.ShowPleaseWait(); });
    setHubUrl(window.location.href, document.title);
    $("html, body").animate({ scrollTop: "0" });
    setInterval(() => {
        if (form.get(0).scrollHeight > form.height())
            syncHubFrame();
    }, 5000);
});
function setHubUrl(url, title) {
    notifyHub("setServiceUrl", { url: url, title: title });
}
function syncHubFrame() {
    notifyHub('setViewFrameHeight', $("form").height());
    notifyHub('setViewFrameWidth', measureWidth($("form")));
}
function measureWidth(item) {
    var pos = item.css("position"); // save original value
    item.css("position", "absolute");
    var width = item.width();
    item.css("position", pos);
    return width;
}
function notifyHub(command, arg) {
    let paramW = { command: command, arg: arg };
    window.parent.postMessage(JSON.stringify(paramW), "*");
}
//# sourceMappingURL=webForms.js.map