# Olive.Mvc.CKEditorFileManager

### #Modal project
Add an entity to your project implementing `Olive.Mvc.CKEditorFileManager.ICKEditorFile`.
```c#
class CKEditorFile : EntityType
{
    public CKEditorFile()
    {
        Implements("Olive.Mvc.CKEditorFileManager.ICKEditorFile");

        SecureFile("File");
    }
}
```

### Website project
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
            <li><a class="ckeditor-file-uri" data-download-uri="@item.Uri">@item.CKEditorFile.File.Filename</a></li>
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
    
    // optional: you can set the size for the brower window as following.
    config.filebrowserWindowWidth = '300';
    config.filebrowserWindowHeight = '50%';
}
```
