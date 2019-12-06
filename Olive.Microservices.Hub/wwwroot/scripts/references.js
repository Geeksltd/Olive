window["BaseThemeUrl"] = 'http://localhost:9011'; // Used in all microservices => should be absolute

requirejs.config({
    urlArgs: "v1.27", // Increment with every release to refresh browser cache.
    baseUrl: window["BaseThemeUrl"] + "/lib", // comes from launchsettings
    paths: {
        // JQuery:
        "jquery": "jquery/dist/jquery",
        "jquery-ui/ui/widget": "jquery-ui/ui/widget",
        "jquery-ui/ui/focusable": "jquery-ui/ui/focusable",
        "jquery-validate": "jquery-validation/dist/jquery.validate",
        "jquery-validate-unobtrusive": "jquery-validation-unobtrusive/jquery.validate.unobtrusive",

        // Jquery plugins:
        "alertify": "alertifyjs/dist/js/alertify",
        "smartmenus": "smartmenus/src/jquery.smartmenus",
        "file-upload": "jquery-file-upload/js/jquery.fileupload",
        "jquery-typeahead": "jquery-typeahead/dist/jquery.typeahead.min",
        "combodate": "combodate/src/combodate",
        "js-cookie": "js-cookie/src/js.cookie",
        "handlebars": "handlebars/handlebars",
        "hammerjs": "hammer.js/hammer",
        "jquery-mentions": "jquery-mentions-input/jquery.mentionsInput",
        "chosen": "chosen-js/chosen.jquery",

        // Bootstrap
        "popper": "popper.js/dist/umd/popper",
        "bootstrap": "bootstrap/dist/js/bootstrap",
        "validation-style": "jquery-validation-bootstrap-tooltip/jquery-validate.bootstrap-tooltip",
        "file-style": "bootstrap-filestyle/src/bootstrap-filestyle",
        "spinedit": "bootstrap-spinedit/js/bootstrap-spinedit",
        "password-strength": "pwstrength-bootstrap/dist/pwstrength-bootstrap-1.2.7",
        "slider": "seiyria-bootstrap-slider/dist/bootstrap-slider.min",
        "moment": "moment/min/moment.min",
        "moment-locale": "moment/locale/en-gb",
        "datepicker": "eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker",
        "bootstrapToggle": "bootstrap-toggle/js/bootstrap-toggle",
        "bootstrap-select": "bootstrap-select/dist/js/bootstrap-select"
    },
    map: {
        "*": {
            "popper.js": "popper",
            '../moment': 'moment',
            'olive': "olive.mvc/dist",
            "app": "../scripts",
            "jquery-sortable": "jquery-ui/ui/widgets/sortable"
        }
    },
    shim: {
        "bootstrap": ["jquery", "popper"],
        "bootstrap-select": ['jquery', 'bootstrap'],
        "bootstrapToggle": ["jquery"],
        "jquery-validate": ['jquery'],
        "validation-style": ['jquery', "jquery-validate", "bootstrap"],
        "combodate": ['jquery'],
        "jquery-typeahead": ['jquery'],
        "file-upload": ['jquery', 'jquery-ui/ui/widget'],
        "file-style": ["file-upload"],
        "smartmenus": ['jquery'],
        "chosen": ['jquery'],
        "jquery-validate-unobtrusive": ['jquery-validate'],
        'backbone.layoutmanager': ['backbone'],
        "spinedit": ['jquery'],
        "password-strength": ['jquery'],
        "moment-locale": ['moment'],
        "olive/extensions/jQueryExtensions": {
            deps: ['jquery', "jquery-validate-unobtrusive"],
            exports: '_'
        },
        "olive/olivePage": ["alertify", "olive/extensions/jQueryExtensions", "olive/plugings/olive-plugins", "olive/extensions/systemExtensions", "combodate"],

        "app/appPage": ["jquery", "olive/olivePage"],

        "app/model/service": ["app/appPage", "olive/extensions/systemExtensions"],
        "app/featuresMenu/featuresMenu": ["app/model/service"],
        "app/featuresMenu/breadcrumbMenu": ["app/featuresMenu/featuresMenu"],
        "app/hub": ["app/featuresMenu/breadcrumbMenu"],
        "jquery-mentions": ['jquery', "underscore/underscore-min", "jquery-mentions-input/lib/jquery.elastic", "jquery-mentions-input/lib/jquery.events.input"]
    }
});

requirejs(["app/hub", "app/appPage",
    // JQuery:
    "jquery-ui/ui/widget", "jquery-ui/ui/focusable", "jquery-validate", "jquery-validate-unobtrusive",
    // JQuery plugins:
    "alertify", "smartmenus", "file-upload", "jquery-typeahead", "js-cookie", "handlebars", "hammerjs", "chosen",
    // Bootstrap and plugins:
    "popper", "bootstrap", "moment", "moment-locale", "datepicker",
    "spinedit", "password-strength", "slider", "file-style", "validation-style", "bootstrapToggle", "bootstrap-select"
]);

window.loadModule = function (path, onLoaded) {
    requirejs([path], function (m) { if (onLoaded) onLoaded(m) });
};