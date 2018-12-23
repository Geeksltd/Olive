# Olive Ajax Lifecycle

## Ajax Based Navigation

Normally when the user click on a like `<a href="..." />` a full page request will run and all javascript files, CSS, images, etc., will be sent to the user and browser should download all the content each time of each request. But usually most page parts share the same content like footer, header, CSS and etc. It is much more efficient to just replace the main content using **Ajax**. Olive framework has facilitated this part by bringing [SPA](https://en.wikipedia.org/wiki/Single-page_application) experience for users with the help of *jQuery*.

### OlivePage.js

Initially by running a project, *Olive framework* load `olivePage.js`. [olivePage.ts](https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/olivePage.ts#L119) is an entry point for application and configure all related javascripts configurations. Olive initially check whole page content for any `<a />` tag with `data-redirect="ajax"` attribute and observe and replace their default action with an Ajax call that just replace `<main />` tag of the page. This tag is default body content and by using jQuery Ajax **Olive** just replace the content of the page and other page parts remain the same. **Olive** will do all jobs just by calling `AjaxRedirect.enableRedirect($("a[data-redirect=ajax]"));` method in `olivePage.js` page.

#### olivePage.ts

```typescript
import Config from "olive/config"
[...]

export default class OlivePage {

    public modal = Modal;
    public waiting = Waiting;

    constructor() {
        SystemExtensions.initialize();
        Modal.initialize();

        [...]

    initialize() {
        
        [...]

        // =================== Request lifecycle ====================
        
        // This method configures browser history and back button
        AjaxRedirect.enableBack($(window));
        
        // This method configures all Ajax navigation
        AjaxRedirect.enableRedirect($("a[data-redirect=ajax]"));

        [...]
    }

    [...]
}
```

### Server Side Response Process

When user click on any navigation links, a jQery Ajax `get` call will be sent to the server from [ajaxRedirect](https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/mvc/ajaxRedirect.ts#L42) and depending on what has been configured in the project page, by calling `Layout(Layouts.FrontEnd);` method M# will use selected template, for example, in `FrontEnd` template M# decide that is this request an Ajax call or not and set page layout accordingly.

```html
@{Layout = Request.IsAjaxCall() ? null : "~/Views/Layouts/FrontEnd.Container.cshtml";}
@{ var leftMenu = ViewData["LeftMenu"].ToStringOrEmpty(); }

<main>
    <header> @await Component.InvokeAsync(typeof(Header)) </header>

    <div class="container-fluid">
        <div class="page">
            @if (leftMenu.HasValue())
            {
                <div class="side-bar"> @await Component.InvokeAsync(ViewData["LeftMenu"].ToString()) </div>
            }

            <div class="content @("full".OnlyWhen(leftMenu.IsEmpty()))">@RenderBody()</div>
        </div>
    </div>

    <footer>@await Component.InvokeAsync(typeof(Footer))</footer>

    @if (!Request.IsAjaxRequest())
    {
        <script src="/lib/requirejs/require.js" data-main="/scripts/references"></script>
    }

    @Html.RegisterDynamicScriptFiles()
    @RenderSection("scripts", required: false)
    @Html.WebTestWidget()
    @Html.RegisterStartupActions()
    @Html.RunJavascript("page.ensureNonModal();")
</main>
```

For Ajax request there is no layout so javascript files, CSS, images, etc., will not load each time and just the content of the page will be replaced. Replacing content will be happening in `processAjaxResponse()` method of [formAction.ts](https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/mvc/formAction.ts#L106);

### Using Browser Back Button

When a user click on back button *Olive* will set current page breadcrumb with its URL to the browser history and by clicking browser *back* button *Olive* remove that URL from history and move back to the previous browser history and set page title according to the  current `page_meta_title` value: `document.title = $("#page_meta_title").val();`

## Ajax Based Form Submission

When a user submits a form, *Olive* will user jQuery Ajax to post the form value to the related controller. *Olive* will use **invokeWithAjax()** method of [formAction](https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/mvc/formAction.ts#L52) to send data. According to the written logic in **[Form Module](https://github.com/Geeksltd/MSharp.Docs/blob/master/Basics/Concepts.md)** of the entity output should be different and all response has been handled in `runAll()` method of the [standardAction](https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/mvc/standardAction.ts#L27)