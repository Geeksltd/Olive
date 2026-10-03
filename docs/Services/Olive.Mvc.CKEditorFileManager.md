# Olive.Mvc.CKEditorFileManager

## Overview

The Olive CKEditor File Manager component provides robust functionality for managing file uploads, browsing, and downloading within applications that utilize the CKEditor rich-text editor. It simplifies integrating file-management capabilities seamlessly with CKEditor, enabling developers to implement quickly and efficiently.

---

## Table of Contents

- [Key Features](#key-features)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Registration and Configuration](#registration-and-configuration)
- [Implementation Guide](#implementation-guide)
  - [Defining the File Entity](#defining-the-file-entity)
  - [File Uploading](#file-uploading)
  - [File Browsing](#file-browsing)
  - [File Downloading](#file-downloading)
- [Core Classes](#core-classes)
  - [FileManagerController](#filemanagercontroller)
  - [Extension Methods](#extension-methods-mvc-registration)
  - [ICKEditorFile Interface](#ickeditorfile-interface)
  - [ViewModel and DTO](#viewmodel-and-dto-classes)
- [Usage Examples](#usage-examples)
- [Dependencies](#dependencies)

---

## Key Features

- **Easy CKEditor Integration**: Works smoothly with the popular CKEditor HTML editor for file uploads/downloads within your editor instances.
- **File Upload Management**: Allows secure, efficient file uploads directly from CKEditor.
- **View and Download Files**: Comes with built-in functionalities to browse and securely download stored files.
- **Database Storage Abstraction**: Seamlessly integrates with Olive's entity system for storing and managing files via the DB adapter.

---

## Getting Started

### Installation

For your Olive application, ensure the following package is installed to manage CKEditor files:

```
Install-Package Olive.Mvc.CKEditorFileManager
```

### Registration and Configuration

In your ASP.NET Core application's `Startup.cs`:

1. Register the CKEditor File Manager with MVC:

```csharp
protected override void ConfigureMvc(IMvcBuilder mvc)
{
    ...
    mvc.AddCKEditorFileManager();
    ...
}
```

2. Define and register the implementer of `ICKEditorFile` interface (an Olive IEntity):

See the [Defining the File Entity](#defining-the-file-entity) section below.

---

## Implementation Guide

### Defining the File Entity

Define your custom entity implementing `ICKEditorFile` to store CKEditor files in your database:

```csharp
class CKEditorFile : EntityType
{
    public CKEditorFile()
    {
        Implements("Olive.Mvc.CKEditorFileManager.ICKEditorFile");

        SecureFile("File");
    }
}
```

### File Uploading

Upload files directly to the database via the built-in controller (`FileManagerController`) provided by this library.

- Uploads automatically store received files as Blob entities implementing `ICKEditorFile`.

File upload is mapped to the route:

```
/ckeditorfileupload
```

Configure CKEditor to use this route for file uploads.

### File Browsing

The file browsing feature lets users browse files previously uploaded via CKEditor.

Access the file browser at:

```
/ckeditorfilebrowser
```

Displays a simple view of uploaded files for easy selection. Integrate this with your CKEditor instance for image selection, hyperlink insertion, etc.

### File Downloading

Files can be securely downloaded using the URL:

```
/ckeditorfiledownload/{filename}
```

These URLs are safely generated and included automatically within the file browsing view.

---

## Core Classes

### FileManagerController

Handles the uploading, browsing, and downloading of CKEditor files:

- **Upload**: Handles POST requests at `/ckeditorfileupload`.
- **Browser**: Handles GET requests at `/ckeditorfilebrowser`.
- **Download**: Handles GET requests at `/ckeditorfiledownload/{filename}`.

### Extension Methods (MVC Registration)

Provides easy registration in your `Startup.cs`:

```csharp
services.AddMvc().AddCKEditorFileManager();
```

This ensures controllers from Olive.MVC.CKEditorFileManager are properly loaded.

### ICKEditorFile Interface

A simple Olive entity interface defining the CKEditor file descriptor:

```csharp
public interface ICKEditorFile : IEntity
{
    Blob File { get; set; }
}
```

### ViewModel and DTO Classes

These classes (`ViewModel`, `DownloadableFileDto`) hold the file metadata and download URLs when displaying the file browser:

```csharp
public class ViewModel : IViewModel
{
    public DownloadableFileDto[] Files { get; set; }
}

public class DownloadableFileDto
{
    public ICKEditorFile CKEditorFile { get; set; }
    public string Uri { get; set; }
}
```

---

## Usage Examples

### Submitting a File to CKEditor:

Finally, add the following lines to your CKEditor's config file.

```javascript
CKEDITOR.editorConfig = function (config) {
    ...
    config.filebrowserBrowseUrl = '/ckeditorfilebrowser';
    config.filebrowserUploadUrl = '/ckeditorfileupload';
    
    // optional: you can set the size for the brower window as following.
    config.filebrowserWindowWidth = '300';
    config.filebrowserWindowHeight = '50%';
}
```

### Accessing Uploaded Files:

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

---

## Dependencies

To use Olive CKEditor File Manager, add these dependencies via NuGet or your project file:
- Olive
- Olive.Entities
- Microsoft.Extensions.DependencyInjection
- Microsoft.AspNetCore.Mvc

```
Install-Package Olive.Mvc.CKEditorFileManager
Install-Package Olive
Install-Package Olive.Entities
```