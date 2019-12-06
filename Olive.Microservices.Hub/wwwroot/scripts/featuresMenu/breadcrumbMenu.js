define(["require", "exports"], function (require, exports) {
    Object.defineProperty(exports, "__esModule", { value: true });
    class BreadcrumbMenu {
        constructor(ajaxRedirect) {
            this.ajaxRedirect = ajaxRedirect;
        }
        enableBreadcrumb() {
            this.bindFeatureMenuItemsClicks($(".features-side-menu .feature-menu-item > a:not([href=''])"));
            this.initBreadcrumb();
        }
        bindItemListClick() {
            //select feature items
            this.bindFeatureMenuItemsClicks($("div.item > a:not([href=''])"));
            //select top menus
            this.bindFeatureMenuItemsClicks($(".feature-top-menu .feature-menu-item > a:not([href=''])"));
        }
        bindFeatureMenuItemsClicks(selector) {
            selector.each((ind, el) => {
                let link = $(el);
                link.click(e => this.onLinkClicked(link));
            });
        }
        onLinkClicked(link) {
            $(".breadcrumb").show().empty();
            this.generateBreadcrumb(link);
            this.ajaxRedirect.enableRedirect($("a[data-redirect=ajax]"));
            $(".breadcrumb-item > a").each((ind, el) => {
                let link = $(el);
                link.click(e => {
                    e.preventDefault();
                    this.onBreadcrumbLinkClicked(link);
                });
            });
        }
        onBreadcrumbLinkClicked(link) {
            let parent = $(".breadcrumb").find(link).parent();
            parent.nextAll().remove();
            var leftMenu = $(".feature-menu-item").find(`a[href="${link[0]["pathname"]}"]`);
            leftMenu.parent().addClass("active");
            if (parent.siblings().length <= 1) {
                $(".features-sub-menu").empty();
            }
            else {
                $("li[data-nodeid=\"" + link.attr("data-itemid") + "\"] li").removeClass("active");
            }
            if (parent.siblings().length > 0) {
                let text = link.text();
                $(".breadcrumb").append(`<li class="breadcrumb-item active" aria-current="page">${text}</li>`);
            }
            parent.remove();
            if ($(".breadcrumb li").length == 1) {
                $(".feature-menu-item").attr("expand", "false");
            }
        }
        initBreadcrumb() {
            let link = $($(".feature-menu-item .active").children("a")[0]);
            if (link.length == 0) {
                link = $($("[expand='true']")[0]).children("a");
            }
            this.generateBreadcrumb(link);
        }
        generateBreadcrumb(link) {
            $(".breadcrumb").append(`<li class="breadcrumb-item"><a href="${window.location.origin}/under/" data-redirect="ajax">Home</a></li>`);
            //check to see if click event is from mid-page or left page
            if (link.parents("li").length == 0) {
                link = $(`#${link.attr("id")} > a`);
            }
            else {
                //find left menu link
                if (link.parents("div.features-side-menu").length === 0) {
                    link = $($(`#${link.parent("li").attr("data-nodeid")}`).find("a")[0]);
                }
            }
            let items = this.removeDuplicate(link.parents("li").get().reverse());
            $(items).each((i, s) => {
                let path = $(s).children("a").attr("href");
                let text = $(s).children("a").text();
                let nodeId = $(s).attr("id");
                if ((items.length - 1) > i) {
                    {
                        let li = $(`<li class="breadcrumb-item"><a href="${path}" data-redirect="ajax" data-itemid="${nodeId}">${text}</a></li>`)
                            .appendTo($(".breadcrumb"));
                        if (!path.startsWith("/under/"))
                            li.find("a").removeAttr("data-redirect");
                    }
                }
                else {
                    $(".breadcrumb").append(`<li class="breadcrumb-item active" aria-current="page">${text}</li>`);
                }
            });
        }
        removeDuplicate(items) {
            let result = [];
            $.each(items, (i, e) => {
                var matchingItems = $.grep(result, function (item) {
                    return item.id === e.id;
                });
                if (matchingItems.length === 0) {
                    result.push(e);
                }
            });
            return result;
        }
    }
    exports.default = BreadcrumbMenu;
});
//# sourceMappingURL=breadcrumbMenu.js.map