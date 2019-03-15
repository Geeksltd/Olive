# Olive.Mvc.CKEditorFileManager

Add the following line to the `ConfigureMvc` in the `Startup.cs`.
```C#
protected override void ConfigureMvc(IMvcBuilder mvc)
{
    ...
    mvc.AddCKEditorFileManager();
    ...
}
```

Add the `CKEditorFileBrowser.cshtml` view file to your shared view folder contianing somthing like bellow. This file could contain anything you like but `@model Olive.Mvc.CKEditorFileManager.ViewModel`, `class="ckeditor-file-uri"` and `data-download-uri="@item.Uri"` are mandatories part.
```cshtml
@model Olive.Mvc.CKEditorFileManager.ViewModel
@{Layout = "~/Views/Layouts/FrontEnd.Modal.cshtml";}

<div>
    <ul>
        @foreach (var item in Model.Files)
        {
            <li><a class="ckeditor-file-uri" data-download-uri="@item.Uri">@item.Filename</a></li>
        }
    </ul>
</div>
```

Finally, add the following lines to your CKEditor's config file.
```js
CKEDITOR.editorConfig = function (config) {
    ...
    config.filebrowserBrowseUrl = '/ckeditorfilebrowser';
    config.filebrowserUploadUrl = '/ckeditorfileupload';
}
```