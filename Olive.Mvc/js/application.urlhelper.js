var UrlHelper = (function () {
    function UrlHelper() {
    }
    UrlHelper.prototype.current = function () { return window.location.href; };
    UrlHelper.prototype.goBack = function () {
        var returnUrl = this.getQuery("ReturnUrl");
        if (returnUrl)
            window.location.href = returnUrl;
        else
            history.back();
    };
    UrlHelper.prototype.pathAndQuery = function () { return window.location.pathname + window.location.search; };
    UrlHelper.prototype.updateQuery = function (uri, key, value) {
        if (uri == null)
            uri = window.location.href;
        var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
        var separator = uri.indexOf('?') !== -1 ? "&" : "?";
        if (uri.match(re)) {
            return uri.replace(re, '$1' + key + "=" + value + '$2');
        }
        else {
            return uri + separator + key + "=" + value;
        }
    };
    UrlHelper.prototype.removeQuery = function (url, parameter) {
        //prefer to use l.search if you have a location/link object
        var urlParts = url.split('?');
        if (urlParts.length >= 2) {
            var prefix = encodeURIComponent(parameter).toLowerCase() + '=';
            var parts = urlParts[1].split(/[&;]/g);
            //reverse iteration as may be destructive
            for (var i = parts.length; i-- > 0;) {
                //idiom for string.startsWith
                if (parts[i].toLowerCase().lastIndexOf(prefix, 0) !== -1) {
                    parts.splice(i, 1);
                }
            }
            url = urlParts[0] + '?' + parts.join('&');
            return url;
        }
        else {
            return url;
        }
    };
    UrlHelper.prototype.getQuery = function (name, url) {
        if (url === void 0) { url = null; }
        if (url)
            url = this.fullQueryString(url);
        else
            url = location.search;
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)", "i"), results = regex.exec(url);
        return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    };
    UrlHelper.prototype.fullQueryString = function (url) {
        if (url == undefined || url == null)
            url = this.current();
        if (url.indexOf("?") == -1)
            return '';
        return url.substring(url.indexOf("?") + 1);
    };
    UrlHelper.prototype.addQuery = function (url, key, value) { return url + (url.indexOf("?") == -1 ? "?" : "&") + key + "=" + value; };
    UrlHelper.prototype.removeEmptyQueries = function (url) {
        var items = this.fullQueryString(url).split('&');
        var result = '';
        for (var i in items) {
            var key = items[i].split('=')[0];
            var val = items[i].split('=')[1];
            if (val != '' && val != undefined)
                result += "&" + key + "=" + val;
        }
        if (items.length > 0)
            result = result.substring(1);
        if (url.indexOf('?') > -1)
            result = url.substring(0, url.indexOf('?') + 1) + result;
        else
            result = url;
        if (result.indexOf("?") == result.length - 1)
            result = result.substring(0, result.length - 1);
        return result;
    };
    UrlHelper.prototype.mergeFormData = function (items) {
        var result = [];
        var a = Array;
        var groupedByKeys = a.groupBy(items, function (i) { return i.name.toLowerCase(); });
        for (var i in groupedByKeys) {
            var group = groupedByKeys[i];
            if (typeof (group) == 'function')
                continue;
            var key = group[0].name;
            var values = group.map(function (item) { return item.value; }).filter(function (v) { return v; });
            // Fix for MVC checkboxes:
            if ($("input[name='" + key + "']").is(":checkbox") && values.length == 2 && values[1] == 'false'
                && (values[0] == 'true' || values[0] == 'false'))
                values.pop();
            result.push({ name: key, value: values.join("|") });
        }
        return result;
    };
    UrlHelper.prototype.htmlEncode = function (html) {
        var a = document.createElement('a');
        a.appendChild(document.createTextNode(html));
        return a.innerHTML;
    };
    UrlHelper.prototype.htmlDecode = function (html) {
        var a = document.createElement('a');
        a.innerHTML = html;
        return a.textContent;
    };
    return UrlHelper;
}());
urlHelper = new UrlHelper();
