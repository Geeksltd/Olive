// For ckeditor plug-ins to work, this should be globally defined.
var CKEDITOR_BASEPATH = '/bower_components/ckeditor/';

class BaseApplicationPage {

    // formats: http://momentjs.com/docs/#/displaying/format/
    DATE_FORMAT = "DD/MM/YYYY";
    TIME_FORMAT = "HH:mm";
    DATE_TIME_FORMAT = "DD/MM/YYYY HH:mm";
    MINUTE_INTERVALS = 5;
    DISABLE_BUTTONS_DURING_AJAX = false;
    DATE_LOCALE = "en-gb";
    REDIRECT_SCROLLS_UP = true;
    AUTOCOMPLETE_INPUT_DELAY = 500;

    /* Possible values: Compact | Medium | Advance | Full
       To customise modes, change '/Scripts/Lib/ckeditor_config.js' file
       */
    DEFAULT_HTML_EDITOR_MODE = "Medium";
    DEFAULT_MODAL_BACKDROP = "static";

    constructor() {
        $(() => {
            $.fn.modal.Constructor.DEFAULTS.backdrop = this.DEFAULT_MODAL_BACKDROP;
            this.enableAlert();
            this.configureValidation();
            this.pageLoad();
        });
    }

    _initializeActions = [];
    onInit(action) { this._initializeActions.push(action) }

    _preInitializeActions = [];
    onPreInit(action) { this._preInitializeActions.push(action) }

    //#region "Events"
    events: { [event: string]: Function[] } = {};

    on(event: string, handler: Function) {
        if (!this.events.hasOwnProperty(event)) this.events[event] = [];
        this.events[event].push(handler);
    }

    raise(event: string, data?: any) {
        let result = true;

        if (this.events.hasOwnProperty(event)) {
            this.events[event].forEach(handler => {
                let res = handler(data || {});
                if (res === false) result = false;
            });
        }
        return result;
    }
    //#endregion "Events"   

    pageLoad(container: JQuery = null, trigger: any = null) {
        $('[autofocus]:not([data-autofocus=disabled]):first').focus();
        this.initializeUpdatedPage(container, trigger);
        if (this.REDIRECT_SCROLLS_UP) $(window).scrollTop(0);
    }

    initializeUpdatedPage(container: JQuery = null, trigger: any = null) {
        this.runStartupActions(container, trigger, "PreInit");
        this.initialize();
        this.runStartupActions(container, trigger, "Init");
    }

    initialize() {

        this._preInitializeActions.forEach((action) => action());

        // =================== Standard Features ====================
        $(".select-cols .apply").off("click.apply-columns").on("click.apply-columns", (e) => this.applyColumns(e));
        $("[data-delete-subform]").off("click.delete-subform").on("click.delete-subform", (e) => this.deleteSubForm(e));
        $("[target='$modal'][href]").off("click.open-modal").on("click.open-modal", (e) => this.openLinkModal(e));
        $(".select-grid-cols .group-control").each((i, e) => this.enableSelectColumns($(e)));
        $("[name=InstantSearch]").each((i, e) => this.enableInstantSearch($(e)));
        $("th.select-all > input:checkbox").off("click.select-all").on("click.select-all", (e) => this.enableSelectAllToggle(e));
        $("[data-user-help]").each((i, e) => this.enableUserHelp($(e)));
        $("form input, form select").off("keypress.default-button").on("keypress.default-button", (e) => this.handleDefaultButton(e));
        $("form[method=get] .pagination-size").find("select[name=p],select[name$='.p']").off("change.pagination-size").on("change.pagination-size", (e) => this.paginationSizeChanged(e));
        $("[data-sort-item]").parents("tbody").each((i, e) => this.enableDragSort($(e)));
        $("a[data-pagination]").off("click.ajax-paging").on("click.ajax-paging", (e) => this.enableAjaxPaging(e));
        $("a[data-sort]").off("click.ajax-sorting").on("click.ajax-sorting", (e) => this.enableAjaxSorting(e));
        $("iframe[data-adjust-height=true]").off("load.auto-adjust").on("load.auto-adjust", (e) => this.adjustIFrameHeightToContents(e.currentTarget));
        $("th[data-sort]").each((i, e) => this.setSortHeaderClass($(e)));
        $("[data-val-number]").off("blur.cleanup-number").on("blur.cleanup-number", (e) => this.cleanUpNumberField($(e.currentTarget)));
        $("[data-toggle=tab]").off("click.tab-toggle").on("click.tab-toggle", () => this.ensureModalResize());
        $.validator.unobtrusive.parse('form');

        // =================== Plug-ins ====================
        $("input[autocomplete-source]").each((i, e) => this.handleAutoComplete($(e)));
        $("[data-control=date-picker],[data-control=calendar]").each((i, e) => this.enableDateControl($(e)));
        $("[data-control='date-picker|time-picker']").each((i, e) => this.enableDateAndTimeControl($(e)));
        $("[data-control=time-picker]").each((i, e) => this.enableTimeControl($(e)));
        $("[data-control=date-drop-downs]").each((i, e) => this.enableDateDropdown($(e)));
        $("[data-control=html-editor]").each((i, e) => this.enableHtmlEditor($(e)));
        $("[data-control=numeric-up-down]").each((i, e) => this.enableNumericUpDown($(e)));
        $("[data-control=range-slider],[data-control=slider]").each((i, e) => this.enableSlider($(e)));
        $(".file-upload input:file").each((i, e) => this.enableFileUpload($(e)));
        $("[data-confirm-question]").each((i, e) => this.enableConfirmQuestion($(e)));
        $(".password-strength").each((i, e) => this.enablePasswordStengthMeter($(e)));
        $(".with-submenu").each((i, e) => this.enableSubMenus($(e)));

        // =================== Request lifecycle ====================
        $(window).off("popstate.ajax-redirect").on("popstate.ajax-redirect", (e) => this.ajaxRedirectBackClicked(e));
        $("a[data-redirect=ajax]").off("click.ajax-redirect").on("click.ajax-redirect", (e) => this.enableAjaxRedirect(e));
        $('form[method=get]').off("submit.clean-up").on("submit.clean-up", (e) => this.cleanGetFormSubmit(e));
        $("[formaction]").not("[formmethod=post]").off("click.formaction").on("click.formaction", (e) => this.invokeActionWithAjax(e, $(e.currentTarget).attr("formaction")));
        $("[formaction][formmethod=post]").off("click.formaction").on("click.formaction", (e) => this.invokeActionWithPost(e));
        $("[data-change-action]").off("change.data-action").on("change.data-action", (e) => this.invokeActionWithAjax(e, $(e.currentTarget).attr("data-change-action")));
        $("[data-change-action][data-control=date-picker],[data-change-action][data-control=calendar]").off("dp.change.data-action").on("dp.change.data-action", (e) => this.invokeActionWithAjax(e, $(e.currentTarget).attr("data-change-action")));

        this.updateSubFormStates();
        this.adjustModalHeight();

        this._initializeActions.forEach((action) => action());
    }

    skipNewWindows() {
        // Remove the target attribute from links:
        $(window).off('click.SanityAdapter').on('click.SanityAdapter', e => {
            $(e.target).filter('a').removeAttr('target');
        });

        this.openWindow = (url, target) => location.replace(url);
    }

    enableDragSort(container) {

        var isTable = container.is("tbody");
        var items = isTable ? "> tr" : "> li"; // TODO: Do we need to support any other markup?

        container.sortable({
            handle: '[data-sort-item]',
            items: items,
            containment: "parent",
            axis: 'y',
            helper: (e, ui) => {
                // prevent TD collapse during drag
                ui.children().each((i, c) => $(c).width($(c).width()));
                return ui;
            },
            stop: (e, ui) => {

                var dropBefore = ui.item.next().find("[data-sort-item]").attr("data-sort-item") || "";

                var handle = ui.item.find("[data-sort-item]");

                var actionUrl = handle.attr("data-sort-action");
                actionUrl = urlHelper.addQuery(actionUrl, "drop-before", dropBefore);

                this.invokeActionWithAjax({ currentTarget: handle.get(0) }, actionUrl);
            }
        });
    }

    enableSubMenus(menu: any) {
        // Many options are supported: http://www.smartmenus.org/docs/
        // To provide your custom options, set data-submenu-options attribute on the UL tag with a string json settings.

        if (!!menu.attr('data-smartmenus-id')) return; // Already enabled

        menu.addClass("sm");

        if (menu.is(".nav-stacked.dropped-submenu"))
            menu.addClass("sm-vertical");

        var submenuOptions = { showTimeout: 0, hideTimeout: 0 };

        var options = menu.attr("data-submenu-options");
        if (options) submenuOptions = this.toJson(options);

        menu.smartmenus(submenuOptions);
    }

    enablePasswordStengthMeter(container: any) {
        // for configuration options : https://github.com/ablanco/jquery.pwstrength.bootstrap/blob/master/OPTIONS.md

        if (container.find(".progress").length !== 0) return;

        var formGroup = container.closest(".form-group");

        var options = {
            common: {},
            rules: {},
            ui: {
                container: formGroup,
                showVerdictsInsideProgressBar: true,
                showStatus: true,
                showPopover: false,
                showErrors: false,
                viewports: {
                    progress: container
                },
                verdicts: [
                    "<span class='fa fa-exclamation-triangle'></span> Weak",
                    "<span class='fa fa-exclamation-triangle'></span> Normal",
                    "Medium",
                    "<span class='fa fa-thumbs-up'></span> Strong",
                    "<span class='fa fa-thumbs-up'></span> Very Strong"],
            }
        };

        var password = formGroup.find(":password");
        if (password.length == 0) {
            console.log('Error: no password field found for password strength.');
            console.log(container);
        }
        else password.pwstrength(options);
    }

    ensureModalResize() {
        setTimeout(() => this.adjustModalHeight(), 1);
    }

    configureValidation() {

        var methods: any = $.validator.methods;

        var format = this.DATE_FORMAT;

        methods.date = function (value, element) {
            if (this.optional(element)) return true;
            return moment(value, format).isValid();
        }

        // TODO: datetime, time
    }

    updateSubFormStates() {

        var countItems = (element) => $(element).parent().find(".subform-item:visible").length;

        // Hide removed items
        $("input[name*=MustBeDeleted][value=True]").closest('[data-subform]').hide();

        // hide empty headers
        $(".horizontal-subform thead").each((i, e) => {
            $(e).css('visibility', (countItems(e) > 0) ? 'visible' : 'hidden');
        });

        // Hide add buttons
        $("[data-subform-max]").each((i, e) => {
            var show = countItems(e) < parseInt($(e).attr('data-subform-max'));
            $(e).find("[data-add-subform=" + $(e).attr("data-subform") + "]").toggle(show);
        });

        // Hide delete buttons
        $("[data-subform-min]").each((i, e) => {
            var show = countItems(e) > parseInt($(e).attr('data-subform-min'));
            $(e).find("[data-delete-subform=" + $(e).attr("data-subform") + "]").css('visibility', (show) ? 'visible' : 'hidden');
        });
    }

    enableDateDropdown(input) {
        // TODO: Implement
    }

    enableSelectAllToggle(event) {
        var trigger = $(event.currentTarget);
        trigger.closest("table").find("td.select-row > input:checkbox").prop('checked', trigger.is(":checked"));
    }

    enableInstantSearch(control) {
        // TODO: Make it work with List render mode too.

        control.off("keyup.immediate-filter").on("keyup.immediate-filter", (event) => {

            var keywords = control.val().toLowerCase().split(' ');

            var rows = control.closest('[data-module]').find(".grid > tbody > tr");

            rows.each((index, e) => {
                var row = $(e);
                var content = row.text().toLowerCase();
                var hasAllKeywords = keywords.filter((i) => content.indexOf(i) == -1).length == 0;
                if (hasAllKeywords) row.show(); else row.hide();
            });

        });

        control.on("keydown", e => {
            if (e.keyCode == 13) e.preventDefault();
        });
    }

    validateForm(trigger) {

        if (trigger.is("[formnovalidate]")) return true;

        var form = trigger.closest("form");

        var validator = form.validate();
        if (!validator.form()) {

            var alertUntyped: any = alert;

            if (form.is("[data-validation-style*=message-box]"))
                alertUntyped(validator.errorList.map(err => err.message).join('\r\n'), () => { setTimeout(() => validator.focusInvalid(), 0); });

            validator.focusInvalid();
            return false;
        }

        return true;
    }

    enableConfirmQuestion(button) {
        button.off("click.confirm-question").bindFirst("click.confirm-question", (e) => {
            e.stopImmediatePropagation();
            //return false;
            alertify.set({
                labels: { ok: button.attr('data-confirm-ok') || 'OK', cancel: button.attr('data-confirm-cancel') || 'Cancel' }
            });
            this.showConfirm(button.attr('data-confirm-question'), () => {
                button.off("click.confirm-question");
                button.trigger('click');
                this.enableConfirmQuestion(button);
            });
            return false;
        });
    }

    showConfirm(text, yesCallback) {
        alertify.confirm(text.replace(/\r/g, "<br />"), (e) => {
            if (e) yesCallback();
            else return false;
        });
    }


    enableHtmlEditor(input: any) {
        $.getScript(CKEDITOR_BASEPATH + "ckeditor.js", () => {
            $.getScript(CKEDITOR_BASEPATH + "adapters/jquery.js", () => {
                CKEDITOR.config.contentsCss = CKEDITOR_BASEPATH + 'contents.css';
                var editor = CKEDITOR.replace($(input).attr('id'),
                    {
                        toolbar: $(input).attr('data-toolbar') || this.DEFAULT_HTML_EDITOR_MODE,
                        customConfig: '/Scripts/ckeditor_config.js'
                    });

                editor.on('change', (evt) => evt.editor.updateElement());

                editor.on("instanceReady", (event) => this.adjustModalHeight());
            });
        });
    }

    alertUnobtrusively(message: string, style?: string) {
        alertify.log(message, style);
    }

    enableAlert() {
        var w: any = window;
        w.alert = (text: string, callback) => this.alert(text, null, callback);
    }

    alert(text: string, style?: string, callback?: Function) {

        if (text == undefined) text = "";
        text = text.trim();

        if (text.indexOf("<") != 0) {
            text = text.replace(/\r/g, "<br />");
            alertify.alert(text, callback, style);
        }
        else {
            alertify.alert('', callback, style);
            $('.alertify-message').empty().append($.parseHTML(text));
        }
    }

    enableNumericUpDown(input: any) {
        var min = input.attr("data-val-range-min");
        var max = input.attr("data-val-range-max");
        input.spinedit({
            minimum: parseFloat(min),
            maximum: parseFloat(max),
            step: 1,
        });
    }

    enableFileUpload(input: any) {
        var control = input;
        var container: JQuery = input.closest(".file-upload");
        var del = container.find(".delete-file");
        var idInput = container.find("input.file-id");
        var progressBar = container.find(".progress-bar");

        control.attr("data-url", "/file/upload");

        // Config http://markusslima.github.io/bootstrap-filestyle/ & https://blueimp.github.io/jQuery-File-Upload/

        control.filestyle({ buttonBefore: true });

        container.find('.bootstrap-filestyle > input:text').wrap($("<div class='progress'></div>"));
        container.find('.bootstrap-filestyle > .progress').prepend(progressBar);

        if (idInput.val() != "REMOVE") {
            var currentFile = container.find('.current-file > a');
            var inputControl = container.find('.bootstrap-filestyle > .progress > input:text');
        }

        var currentFileName = currentFile ? currentFile.text() : null;
        var hasExistingFile = currentFileName != "«UNCHANGED»" && (currentFileName != "NoFile.Empty" && currentFileName != null);

        if (hasExistingFile && inputControl.val() == "") {
            del.show();
            progressBar.width('100%');
            // enable Existing File Download
            inputControl.val(currentFile.text()).removeAttr('disabled').addClass('file-target').click(() => currentFile[0].click());
        }

        var handleCurrentFileChange = () => {
            if (hasExistingFile) {
                inputControl.removeClass('file-target').attr('disabled', 'true').off();
                hasExistingFile = false;
            }
        };

        del.click((e) => {
            del.hide();
            idInput.val("REMOVE");
            progressBar.width(0);
            control.filestyle('clear');
            handleCurrentFileChange();
        });

        var fileLabel = control.parent().find(':text');

        input.fileupload({
            dataType: 'json',
            dropZone: container,
            replaceFileInput: false,
            drop: (e, data) => {

                if (fileLabel.length > 0 && data.files.length > 0) {
                    fileLabel.val(data.files.map(x => x.name));
                }
            },
            change: (e, data) => { progressBar.width(0); handleCurrentFileChange(); },
            progressall: (e, data: any) => {
                var progress = parseInt((data.loaded / data.total * 100).toString(), 10);
                progressBar.width(progress + '%');
            },
            error: (response) => { this.handleAjaxResponseError(response); fileLabel.val(''); },
            success: (response) => {
                if (response.Error) {
                    this.handleAjaxResponseError({ responseText: response.Error });
                    fileLabel.val('');
                }
                else {
                    if (input.is("[multiple]")) idInput.val(idInput.val() + "|file:" + response.ID);
                    else idInput.val("file:" + response.ID);
                    del.show();
                }
            }
        });
    }

    openLinkModal(event: JQueryEventObject) {

        var target = $(event.currentTarget);
        var url = target.attr("href");

        var modalOptions = {};

        var options = target.attr("data-modal-options");
        if (options) modalOptions = this.toJson(options);

        this.openModal(url, modalOptions);

        return false;
    }

    toJson(data) {
        try {
            return JSON.parse(data);
        } catch (error) {
            console.log(error);
            console.log('Cannot parse this data to Json: ');
            console.log(data);
        }
    }

    runStartupActions(container: JQuery = null, trigger: any = null, stage: string = "Init") {
        if (container == null) container = $(document);
        if (trigger == null) trigger = $(document);
        var actions = [];
        $("input[name='Startup.Actions']", container).each((index, item) => {
            var action = $(item).val();
            if (actions.indexOf(action) === -1)
                actions.push(action);
        });

        for (var action of actions) {
            if (action && (action.Stage || "Init") == stage) this.executeActions(this.toJson(action), trigger);
        }
    }

    canAutoFocus(input: JQuery) {
        return input.attr("data-autofocus") !== "disabled";
    }

    enableDateControl(input: JQuery) {
        if (this.isWindowModal()) {
            input.off("dp.show.adjustHeight").on("dp.show.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
            input.off("dp.hide.adjustHeight").on("dp.hide.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
        }

        input.attr("data-autofocus", "disabled");

        var control = input.attr("data-control");
        var viewMode = input.attr("data-view-mode") || 'days';

        if (control == "date-picker") {
            (<any>input).datetimepicker({
                format: this.DATE_FORMAT,
                useCurrent: false,
                showTodayButton: true,
                icons: { today: 'today' },
                viewMode: viewMode,
                keepInvalid: input.closest("form").find("[data-change-action]").length == 0,
                locale: this.DATE_LOCALE
            }).data("DateTimePicker").keyBinds().clear = null;

            // Now make calendar icon clickable as well             
            input.parent().find(".fa-calendar").parent(".input-group-addon").click(() => { input.focus(); });

        }
        else alert("Don't know how to handle date control of " + control);
    }

    adjustModalHeightForDataPicker(e) {

        var datepicker = $(e.currentTarget).siblings('.bootstrap-datetimepicker-widget');

        if (datepicker.length === 0) {
            this.adjustModalHeight();
            return;
        }

        var offset = Math.ceil(datepicker.offset().top + datepicker[0].offsetHeight) - document.body.offsetHeight + 6;
        var overflow = Math.max(offset, 0);
        this.adjustModalHeight(overflow);
    }

    enableDateAndTimeControl(input: any) {

        if (this.isWindowModal()) {
            input.off("dp.show.adjustHeight").on("dp.show.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
            input.off("dp.hide.adjustHeight").on("dp.hide.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
        }

        input.attr("data-autofocus", "disabled");

        input.datetimepicker({
            format: this.DATE_TIME_FORMAT,
            useCurrent: false,
            showTodayButton: true,
            icons: { today: 'today' },
            stepping: parseInt(input.attr("data-minute-steps") || this.MINUTE_INTERVALS.toString()),
            keepInvalid: input.closest("form").find("[data-change-action]").length == 0,
            locale: this.DATE_LOCALE
        }).data("DateTimePicker").keyBinds().clear = null;

        input.parent().find(".fa-calendar").click(function () { input.focus(); });
    }

    enableTimeControl(input: any) {

        if (this.isWindowModal()) {
            input.off("dp.show.adjustHeight").on("dp.show.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
            input.off("dp.hide.adjustHeight").on("dp.hide.adjustHeight", (e) => this.adjustModalHeightForDataPicker(e));
        }

        input.attr("data-autofocus", "disabled");

        input.datetimepicker({
            format: this.TIME_FORMAT,
            useCurrent: false,
            stepping: parseInt(input.attr("data-minute-steps") || this.MINUTE_INTERVALS.toString()),
            keepInvalid: input.closest("form").find("[data-change-action]").length == 0,
            locale: this.DATE_LOCALE
        }).data("DateTimePicker").keyBinds().clear = null;

        input.parent().find(".fa-clock-o").parent(".input-group-addon").click(() => { input.focus(); });
    }

    awaitingAutocompleteResponses: number = 0;
    handleAutoComplete(input) {
        if (input.is('[data-typeahead-enabled=true]')) return;
        else input.attr('data-typeahead-enabled', true);

        var valueField = $("[name='" + input.attr("name").slice(0, -5) + "']");

        if (valueField.length == 0) console.log('Could not find the value field for auto-complete.');

        var dataSource = (query, callback) => {
            this.awaitingAutocompleteResponses++;

            var url = input.attr("autocomplete-source");
            url = urlHelper.removeQuery(url, input.attr('name')); // Remove old text.
            var data = this.getPostData(input);

            setTimeout(() => {
                if (this.awaitingAutocompleteResponses > 1) {
                    this.awaitingAutocompleteResponses--
                    return;
                }

                $.post(url, data).fail(this.handleAjaxResponseError).done((result) => {

                    result = result.map((i) => {
                        return {
                            Display: i.Display || i.Text || i.Value,
                            Value: i.Value || i.Text || i.Display,
                            Text: i.Text || $("<div/>").append($(i.Display)).text() || i.Value
                        };
                    });

                    return callback(result);
                }).always(() => this.awaitingAutocompleteResponses--);
            }, this.AUTOCOMPLETE_INPUT_DELAY);
        };

        var clearValue = (e) => {
            if (input.val() === "") valueField.val("");
            if (input.val() !== input.data("selected-text")) valueField.val("");
        };

        var itemSelected = (e, item) => {
            if (item != undefined) {
                console.log('setting ' + item.Value);
                valueField.val(item.Value);
                input.data("selected-text", item.Display);
            }
            else {
                console.log("Clearing text, item is undefined");
                input.data("selected-text", "");
            }

            // This will invoke RunOnLoad M# method as typeahead does not fire textbox change event when it sets its value from drop down
            input.trigger('change');
        };

        var itemBlured = (e, item) => {
            if (valueField.val() == "" && input.val() != "") {
                // this hack is so when you paste something a focus out, it should set the hidden field
                var suggested = input.closest(".twitter-typeahead").find(".tt-suggestion");
                var filtered = suggested.filter((e, obj) => (obj.innerText === input.val()));

                if (filtered.length === 0 && suggested.length === 0) {
                    // the suggestion list has never been shown

                    // make typeahead aware of this change otherwise during blur it will clear the text
                    input.typeahead('val', input.val());
                    dataSource(input.val(), data => {
                        if (data && data.length === 1) {
                            itemSelected(null, data[0]);
                            console.log('match text to suggestion finished');
                        } else {
                            console.warn("There is none or more than one items in the autocomplete data-source to match the given text. Cannot set the value.");
                        }
                    });
                }
                else {
                    // the suggestion list has been displayed
                    if (filtered.length === 0)
                        suggested.first().trigger("click");
                    else
                        filtered.first().trigger("click");
                }
            }
        };

        var dataset = {
            displayKey: 'Text', source: dataSource,
            templates: { suggestion: (item) => item.Display, empty: "<div class='tt-suggestion'>Not found</div>" }
        };

        input.data("selected-text", "").on('input', clearValue).on('blur', itemBlured).on('typeahead:selected', itemSelected).typeahead({ minLength: 0 }, dataset);
    }

    handleDefaultButton(event: JQueryEventObject): boolean {
        if (event.which === 13) {
            var target = $(event.currentTarget);
            var button = target.closest("[data-module]").find('[default-button]:first'); // Same module
            if (button.length == 0) button = $('[default-button]:first') // anywhere
            button.click();
            return false;
        } else return true;
    }

    deleteSubForm(event: JQueryEventObject) {
        var button = $(event.currentTarget);
        var container = button.parents(".subform-item");
        container.find("input[name*=MustBeDeleted]").val("true");
        container.hide();

        this.updateSubFormStates();
        event.preventDefault();
    }

    enableAjaxPaging(event: JQueryEventObject) {
        var button = $(event.currentTarget);
        var page = button.attr("data-pagination");

        var key = "p";

        if (page.split('=').length > 1) { key = page.split('=')[0]; page = page.split('=')[1]; }

        var input = $("[name='" + key + "']");
        input.val(page);
        if (input.val() != page) {
            // Drop down list case
            input.parent().append($("<input type='hidden'/>").attr("name", key).val(page));
            input.remove();
        }
    }

    enableAjaxSorting(event: JQueryEventObject) {
        var button = $(event.currentTarget);
        var sort = button.attr("data-sort");

        var key = "s";

        if (sort.split('=').length > 1) {
            key = sort.split('=')[0];
            sort = sort.split('=')[1];
        }

        var input = $("[name='" + key + "']");
        if (input.val() == sort) sort += ".DESC";

        input.val(sort);
    }

    applyColumns(event: JQueryEventObject) {
        var button = $(event.currentTarget);
        var checkboxes = button.closest(".select-cols").find(":checkbox");

        if (checkboxes.length === 0 || checkboxes.filter(":checked").length > 0) return;

        $("<input type='checkbox' checked='checked'/>").hide().attr("name", checkboxes.attr("name")).val("-")
            .appendTo(button.parent());
    }

    enableAjaxRedirect(event: JQueryEventObject) {

        if (event.ctrlKey || event.button === 1) return true;

        var link = $(event.currentTarget);
        var url = link.attr('href');

        this.ajaxRedirect(url, link);

        return false;
    }

    ajaxChangedUrl = 0;
    isAjaxRedirecting = false;
    ajaxRedirect(url: string, trigger: JQuery = null, isBack: boolean = false, keepScroll: boolean = false, addToHistory = true) {
        this.isAjaxRedirecting = true;
        this.isAwaitingAjaxResponse = true;

        if (window.stop) window.stop();
        else if (document.execCommand !== undefined) document.execCommand("Stop", false);

        var scrollTopBefore;
        if (keepScroll) {
            scrollTopBefore = $(document).scrollTop();
        }

        this.showPleaseWait();

        $.ajax({
            url: url,
            type: 'GET',
            success: (response) => {

                this.events = {};

                if (!isBack) {
                    this.ajaxChangedUrl++;
                    if (addToHistory) history.pushState({}, $("#page_meta_title").val(), url);
                }

                this.isAwaitingAjaxResponse = false;
                this.isAjaxRedirecting = false;
                this.invokeAjaxActionResult(response, null, trigger);
                if (keepScroll) {
                    $(document).scrollTop(scrollTopBefore);
                }
            },
            error: (response) => location.href = url,
            complete: (response) => this.hidePleaseWait()
        });

        return false;
    }

    ajaxRedirectBackClicked(event) {

        if (this.ajaxChangedUrl == 0) return;

        this.ajaxChangedUrl--;
        this.ajaxRedirect(location.href, null, true);
    }

    returnToPreviousPage(target) {
        var returnUrl = urlHelper.getQuery("ReturnUrl");

        if (returnUrl) {
            if (target && $(target).is("[data-redirect=ajax]")) this.ajaxRedirect(returnUrl, $(target));
            else location.href = returnUrl;
        }
        else history.back();

        return false;
    }

    cleanGetFormSubmit(event: JQueryEventObject) {

        var form = $(event.currentTarget);
        if (this.validateForm(form) == false) { this.hidePleaseWait(); return false; }

        var formData = urlHelper.mergeFormData(form.serializeArray()).filter(item => item.name != "__RequestVerificationToken");

        var url = urlHelper.removeEmptyQueries(form.attr('action'));

        try {

            form.find("input:checkbox:unchecked").each((ind, e) => url = urlHelper.removeQuery(url, $(e).attr("name")));

            for (var item of formData)
                url = urlHelper.updateQuery(url, item.name, item.value);

            url = urlHelper.removeEmptyQueries(url);

            if (form.is("[data-redirect=ajax]")) this.ajaxRedirect(url, form);
            else location.href = url;
        }
        catch (error) {
            console.log(error);
            alert(error);
        }
        return false;
    }

    enableUserHelp(element: JQuery) {

        element.click(() => false);
        var message = element.attr('data-user-help');  // todo: unescape message and conver to html

        element['popover']({ trigger: 'focus', content: message });
    }

    executeActions(actions: any, trigger: any = null) {
        for (var action of actions) {
            if (!this.executeAction(action, trigger)) return;
        }
    }

    executeAction(action: any, trigger: any): boolean {
        if (action.Notify || action.Notify == "") this.executeNotifyAction(action, trigger);
        else if (action.Script) eval(action.Script);
        else if (action.BrowserAction == "Back") window.history.back();
        else if (action.BrowserAction == "CloseModal" && this.closeModal() === false) return false;
        else if (action.BrowserAction == "CloseModalRefreshParent" && this.closeModal(true) === false) return false;
        else if (action.BrowserAction == "Close") window.close();
        else if (action.BrowserAction == "Refresh") this.refresh();
        else if (action.BrowserAction == "Print") window.print();
        else if (action.BrowserAction == "ShowPleaseWait") this.showPleaseWait(action.BlockScreen);
        else if (action.ReplaceSource) this.replaceListControlSource(action.ReplaceSource, action.Items);
        else if (action.Download) this.download(action.Download);
        else if (action.Redirect) this.executeRedirectAction(action, trigger);
        else alert("Don't know how to handle: " + urlHelper.htmlEncode(JSON.stringify(action)));

        return true;
    }

    executeNotifyAction(action: any, trigger: any) {
        if (action.Obstruct == false)
            this.alertUnobtrusively(action.Notify, action.Style);
        else this.alert(action.Notify, action.Style);
    }

    executeRedirectAction(action: any, trigger: any) {
        if (action.Redirect.indexOf('/') != 0 && action.Redirect.indexOf('http') != 0) action.Redirect = '/' + action.Redirect;

        if (action.OutOfModal && this.isWindowModal()) parent.window.location.href = action.Redirect;
        else if (action.Target == '$modal') this.openModal(action.Redirect, {});
        else if (action.Target && action.Target != '') this.openWindow(action.Redirect, action.Target);
        else if (action.WithAjax === false) location.replace(action.Redirect);
        else if ((trigger && trigger.is("[data-redirect=ajax]")) || action.WithAjax == true) this.ajaxRedirect(action.Redirect, trigger);
        else location.replace(action.Redirect);
    }

    replaceListControlSource(controlId: string, items) {

        var $control = $('#' + controlId);

        if ($control.is("select")) {
            $control.empty();
            for (var i = 0; i < items.length; i++) {
                $control.append($("<option value='" + items[i].Value + "'>" + items[i].Text + "</option>"));
            }

        } else {
            console.log("Unable to replace list items");
        }
    }

    download(url: string) {

        if (this.isWindowModal()) {
            var page = window.parent["page"];
            if (page && page.download) {
                page.download(url);
                return;
            }
        }

        $("<iframe style='visibility:hidden; width:1px; height:1px;'></iframe>").attr("src", url).appendTo("body");
    }

    openWindow(url: string, target: string) {
        window.open(url, target);
    }

    hidePleaseWait() {
        $(".wait-screen").remove();
    }

    showPleaseWait(blockScreen: boolean = false) {

        if (!$(document.forms[0]).valid()) return;

        var screen = $("<div class='wait-screen' />").appendTo("body");

        if (blockScreen) {
            $("<div class='cover' />")
                .width(Math.max($(document).width(), $(window).width()))
                .height(Math.max($(document).height(), $(window).height()))
                .appendTo(screen);
        }

        $("<div class='wait-container'><div class='wait-box'><img src='/public/img/loading.gif'/></div>")
            .appendTo(screen)
            .fadeIn('slow');
    }

    currentModal: any = null;

    getModalTemplate(options: any) {

        var modalDialogStyle = "";
        var iframeStyle = "width:100%; border:0;";
        var iframeAttributes = "";

        if (options) {
            if (options.width) {
                modalDialogStyle += "width:" + options.width + ";";
            }

            if (options.height) {
                modalDialogStyle += "height:" + options.height + ";";
                iframeStyle += "height:" + options.height + ";";
                iframeAttributes += " data-has-explicit-height='true'";
            }
        }

        return "<div class='modal fade' id='myModal' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'\
 aria-hidden='true'>\
            <div class='modal-dialog' style='"+ modalDialogStyle + "'>\
    <div class='modal-content'>\
    <div class='modal-header'>\
        <button type='button' class='close' data-dismiss='modal' aria-label='Close'>\
            <i class='fa fa-times-circle'></i>\
        </button>\
    </div>\
    <div class='modal-body'>\
        <div class='row text-center'><i class='fa fa-spinner fa-spin fa-2x'></i></div>\
        <iframe style='"+ iframeStyle + "' " + iframeAttributes + "></iframe>\
    </div>\
</div></div></div>";
    }

    isOpeningModal = false;
    openModal(url: string, options: any = {}) {
        this.isOpeningModal = true;

        if (this.currentModal != null)
            if (this.closeModal() === false) return false;

        this.currentModal = $(this.getModalTemplate(options));

        if (true /* TODO: Change to if Internet Explorer only */)
            this.currentModal.removeClass("fade");

        var frame = this.currentModal.find("iframe");

        frame.attr("src", url).on("load", (e) => {
            this.isOpeningModal = false;

            var isHeightProvided = !!(options && options.height);

            if (!isHeightProvided) {
                var doc = frame.get(0).contentWindow.document;
                setTimeout(() => frame.height(doc.body.offsetHeight), 10); // Timeout is used due to an IE bug.
            }

            this.currentModal.find(".modal-body .text-center").remove();
        });

        this.currentModal.appendTo("body").modal('show');
    }

    isClosingModal = false;
    closeModal(refreshParent = false) {

        if (this.raise("modal:closing") === false) return false;

        this.isClosingModal = true;

        if (this.currentModal) {

            this.currentModal.modal('hide').remove();
            if (refreshParent) this.refresh();
            this.currentModal = null;

            this.raise("modal:closed");
        }
        else if (window.parent) {
            var p: any = window.parent;
            if (p.page) if (p.page.closeModal(refreshParent) === false) return false;
        }

        this.isClosingModal = false;

        return true;
    }

    refresh(keepScroll: boolean = false) {
        if ($("main").parent().is("body"))
            this.ajaxRedirect(location.href, null, false /*isBack*/, keepScroll, false /*addToHistory:*/);
        else location.reload();
    }

    getPostData(trigger: JQuery): JQuerySerializeArrayElement[] {

        var form = trigger.closest("[data-module]");

        if (!form.is("form")) form = $("<form />").append(form.clone(true));

        var data = urlHelper.mergeFormData(form.serializeArray());

        // If it's master-details, then we need the index.
        var subFormContainer = trigger.closest(".subform-item");
        if (subFormContainer != null) {
            data.push({
                name: "subFormIndex",
                value: subFormContainer.closest(".horizontal-subform, .vertical-subform").find(".subform-item").index(subFormContainer).toString()
            });
        }

        data.push({ name: "current.request.url", value: urlHelper.pathAndQuery() });

        return data;
    }

    isAwaitingAjaxResponse = false;
    invokeActionWithAjax(event, actionUrl, syncCall = false) {

        var trigger = $(event.currentTarget);
        var triggerUniqueSelector = trigger.getUniqueSelector();
        var containerModule = trigger.closest("[data-module]");

        if (this.validateForm(trigger) == false) { this.hidePleaseWait(); return false; }

        var data_before_disable = this.getPostData(trigger);

        var disableToo = this.DISABLE_BUTTONS_DURING_AJAX && !trigger.is(":disabled");
        if (disableToo) trigger.attr('disabled', 'disabled');

        trigger.addClass('loading-action-result');

        this.isAwaitingAjaxResponse = true;

        $.ajax({
            url: actionUrl,
            type: trigger.attr("data-ajax-method") || 'POST',
            async: !syncCall,
            data: data_before_disable,
            success: (result) => { this.hidePleaseWait(); this.invokeAjaxActionResult(result, containerModule, trigger); },
            error: (response) => this.handleAjaxResponseError(response),
            complete: (x) => {
                this.isAwaitingAjaxResponse = false;

                trigger.removeClass('loading-action-result');
                if (disableToo) trigger.removeAttr('disabled');

                var triggerTabIndex = $(":focusable").index($(triggerUniqueSelector));
                if (triggerTabIndex > -1) $(":focusable").eq(triggerTabIndex + 1).focus();
            }
        });

        return false;
    }

    enableSelectColumns(container) {
        var columns = container.find("div.select-cols");
        container.find("a.select-cols").click(() => { columns.show(); return false; });
        columns.find('.cancel').click(() => columns.hide());
    }

    invokeActionWithPost(event) {
        var trigger = $(event.currentTarget);
        var containerModule = trigger.closest("[data-module]");

        if (containerModule.is("form") && this.validateForm(trigger) == false) return false;

        var data = this.getPostData(trigger);
        var url = trigger.attr("formaction");
        var form = $("<form method='post' />").hide().appendTo($("body"));

        for (var item of data)
            $("<input type='hidden'/>").attr("name", item.name).val(item.value).appendTo(form);

        form.attr("action", url).submit();
        return false;
    }

    handleAjaxResponseError(response) {
        this.hidePleaseWait();
        console.log(response);

        var text = response.responseText;

        if (text.indexOf("<html") > -1) {
            document.write(text);
        }
        else if (text.indexOf("<form") > -1) {
            var form = $("form", document);
            if (form.length) form.replaceWith($(text));
            else document.write(text);
        }
        else alert(text);
    }

    dynamicallyLoadedScriptFiles = [];

    replaceMain(element: JQuery, trigger) {
        var referencedScripts = element.find("script[src]").map((i, s) => $(s).attr("src"));
        element.find("script[src]").remove();

        $("main").replaceWith(element);

        if (referencedScripts.length) {
            var expectedScripts = referencedScripts.length;
            var loadedScripts = 0;
            referencedScripts.each((index, item) => {
                var url = '' + item;
                if (this.dynamicallyLoadedScriptFiles.indexOf(url) > -1) {
                    loadedScripts++;
                    if (loadedScripts == expectedScripts) this.pageLoad(element, trigger);
                }
                else {
                    this.dynamicallyLoadedScriptFiles.push(url);
                    $.getScript(url, () => {
                        loadedScripts++;
                        if (loadedScripts == expectedScripts) this.pageLoad(element, trigger);
                    });
                }
            });
        }
        else this.pageLoad(element, trigger);

        document.title = $("#page_meta_title").val();
    }

    invokeAjaxActionResult(response, containerModule, trigger) {

        var asElement = $(response);

        if (asElement.is("main")) {
            this.replaceMain(asElement, trigger);
            return;
        }

        if (asElement.is("[data-module]")) {
            // TODO: Support specifying the module to be updated at the Action level.
            containerModule.replaceWith(asElement);

            this.initializeUpdatedPage(asElement, trigger);
        }
        else if (response.length == 1 && response[0].ReplaceView) {
            asElement = $("<div/>").append(response[0].ReplaceView);
            containerModule.replaceWith(asElement);
            this.initializeUpdatedPage(asElement, trigger);
        }
        else if (trigger && trigger.is("[data-add-subform]")) {
            var subFormName = trigger.attr("data-add-subform");
            var container = containerModule.find("[data-subform=" + subFormName + "] > table tbody:first");

            if (container.length == 0) container = containerModule.find("[data-subform=" + subFormName + "]:first");

            container.append(asElement);

            this.reloadValidationRules(trigger.parents("form"));

            this.updateSubFormStates();

            this.initializeUpdatedPage(asElement, trigger);
        }
        else {
            this.executeActions(response, trigger);
            this.initialize();
        }
    }

    ensureNonModal() {
        if (this.isWindowModal())
            parent.window.location.href = location.href;
    }

    isWindowModal() {
        if ($(this.getContainerIFrame()).closest(".modal").length === 0) return false;
        return true;
    }

    getContainerIFrame() {
        if (parent == null || parent == self) return null;
        return $(parent.document).find("iframe").filter((i, f: any) => (f.contentDocument || f.contentWindow.document) == document).get(0);
    }

    cleanJson(str): string {
        return str.replace(/(\s*?{\s*?|\s*?,\s*?)(['"])?([a-zA-Z0-9]+)(['"])?:/g, '$1"$3":')
    };

    enableSlider(input) {
        var options = { min: 0, max: 100, value: null, range: false, formatter: null, tooltip: 'always', upper: null, tooltip_split: false };

        var data_options = input.attr("data-options") ? JSON.parse(this.cleanJson(input.attr("data-options"))) : null;
        if (data_options) $.extend(true, options, data_options);

        options.range = input.attr("data-control") == "range-slider";

        if (options.range) {
            if (options.tooltip_split == false)
                options.formatter = v => v[0] + " - " + v[1];

            if (input.attr("id").endsWith("Max")) return;
            var maxInput = $('[name="' + input.attr("id").split('.')[0] + "." + options.upper + '\"]');
            if (maxInput.length == 0)
                maxInput = $('[name="' + options.upper || input.attr("id") + 'Max' + '\"]');

            if (maxInput.length == 0) throw new Error("Upper input was not found for the range slider.");

            options.value = [Number(input.val() || options.min), Number(maxInput.val() || options.max)];

            // Standard SEARCH min and max.														 
            // TODO: Change the following to first detect if we're in a search control context and skip the following otherwise.
            var container = $(input).closest(".group-control");
            if (container.length == 0) container = input.parent();
            container.children().each((i, e) => $(e).hide());
            var rangeSlider = $("<input type='text' class='range-slider'/>").attr("id", input.attr("id") + "_slider").appendTo(container);
            (<any>rangeSlider).slider(options).on('change', ev => { input.val(ev.value.newValue[0]); maxInput.val(ev.value.newValue[1]); });   ///// Updated ***********
        }
        else {
            options.value = Number(input.val() || options.min);
            (<any>input).slider(options).on('change', ev => { input.val(ev.value.newValue); });  ///// Updated ***********
        }
    }

    adjustModalHeight(overflow?: number) {
        if (this.isWindowModal()) {

            var frame = $(this.getContainerIFrame());
            if (frame.attr("data-has-explicit-height") != 'true')
                frame.height(document.body.offsetHeight + (overflow || 0));
        }
    }

    adjustIFrameHeightToContents(iframe) {
        $(iframe).height(iframe.contentWindow.document.body.scrollHeight);
    }

    reloadValidationRules(form: JQuery) {
        form.removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);
    }

    paginationSizeChanged(event: Event) {
        $(event.currentTarget).closest("form").submit();
    }

    highlightRow(element: any) {
        var target = $(element.closest("tr"));
        target.siblings('tr').removeClass('highlighted');
        target.addClass('highlighted');
    }

    cleanUpNumberField(field: JQuery) {
        var domElement = <HTMLInputElement>field.get(0);

        // var start = domElement.selectionStart;
        // var end = domElement.selectionEnd;

        field.val(field.val().replace(/[^\d.-]/g, ""));

        // domElement.setSelectionRange(start, end);
    }

    setSortHeaderClass(thead: JQuery) {
        var currentSort = thead.closest("[data-module]").find("#Current-Sort").val() || "";

        if (currentSort == "") return;

        var sortKey = thead.attr('data-sort');

        if (sortKey == currentSort && !thead.hasClass('sort-ascending')) {
            thead.addClass("sort-ascending");
            thead.append("<i />");
        }
        else if (currentSort == sortKey + ".DESC" && !thead.hasClass('sort-descending')) {
            thead.addClass("sort-descending");
            thead.append("<i />");
        }
    }
}