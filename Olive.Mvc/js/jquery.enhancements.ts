interface Window { stop(); }

(function () {

    var a: any = Array;

    a.groupBy = (array: Array<any>, groupFunction: Function) => {

        var groups = {};
        array.forEach(function (o) {
            var group = JSON.stringify(groupFunction(o));
            groups[group] = groups[group] || [];
            groups[group].push(o);
        });

        return Object.keys(groups).map((g) => groups[g]);
    };

})();

// [name] is the name of the event "click", "mouseover", .. 
// same as you'd pass it to bind()
// [fn] is the handler function
$.fn.bindFirst = function (name, fn) {
    // bind as you normally would
    // don't want to miss out on any jQuery magic
    this.bind(name, fn);

    // Thanks to a comment by @Martin, adding support for
    // namespaced events too.
    var jq: any = $;

    var eventsData = jq._data(this.get(0), "events");
    if (eventsData) {

        var handlers = eventsData[name.split('.')[0]];
        // take out the handler we just inserted from the end
        var handler = handlers.pop();
        // move it at the beginning
        handlers.splice(0, 0, handler);
    }

    return this;
};


(function (original) {
    jQuery.fn.clone = function () {
        var result = original.apply(this, arguments),
            my_textareas = this.find('textarea').add(this.filter('textarea')),
            result_textareas = result.find('textarea').add(result.filter('textarea')),
            my_selects = this.find('select').add(this.filter('select')),
            result_selects = result.find('select').add(result.filter('select'));

        for (var i = 0, l = my_textareas.length; i < l; ++i) $(result_textareas[i]).val($(my_textareas[i]).val());
        for (var i = 0, l = my_selects.length; i < l; ++i) result_selects[i].selectedIndex = my_selects[i].selectedIndex;

        return result;
    };
})(jQuery.fn.clone);

// Enable jquery Validate for checkbox list
(function ($) {
    $.validator.unobtrusive.adapters.add("selection-required", function (options) {
        if (options.element.tagName.toUpperCase() == "INPUT" && options.element.type.toUpperCase() == "CHECKBOX") {
            var $element = $(options.element);
            options.rules["required"] = true;
            options.messages["required"] = $element.data('valRequired');
        }
    });
}(jQuery));


// Enabling validation for time picker
(function ($) {
    $.validator.addMethod("time", function (value, element, params) {
        return this.optional(element) || /^([01]\d|2[0-3]|[0-9])(:[0-5]\d){1,2}$/.test(value);
    }, 'Please enter a valid time, between 00:00 and 23:59');
    $.validator.unobtrusive.adapters.addBool("time");
}(jQuery));


jQuery.fn.extend({
    screenOffset: function () {
        var documentOffset = this.first().offset();
        return {
            top: documentOffset.top - $(window).scrollTop(),
            left: documentOffset.left - $(window).scrollLeft()
        };
    }
});

jQuery.fn.extend({
    getUniqueSelector: function () {
        if (this.length != 1) throw 'Requires one element.';
        var path, node = this;
        while (node.length) {
            var realNode = node[0],
                name = realNode.localName;
            if (!name) break;
            name = name.toLowerCase();
            var parent = node.parent();
            var siblings = parent.children(name);
            if (siblings.length > 1) {
                name += ':eq(' + siblings.index(realNode) + ')';
            }
            path = name + (path ? '>' + path : '');
            node = parent;
        }
        return path;
    }
});

// -------------------- String Extensions ------------------
interface String { endsWith(searchString: string): boolean; }
if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (searchString) {
        var subjectString = this.toString();
        var position = subjectString.length - searchString.length;
        var lastIndex = subjectString.indexOf(searchString, position);
        return lastIndex !== -1 && lastIndex === position;
    };
}