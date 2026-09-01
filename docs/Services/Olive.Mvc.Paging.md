# Olive MVC Pagination Documentation

## Overview

The **Olive MVC Pagination** component introduces robust, easy-to-integrate pagination functionalities tailored for Olive MVC-based applications. It simplifies the presentation and navigation of large datasets by limiting displayed records per page, providing configurable pagination controls, supporting advanced AJAX interactions, and enabling seamless integration with database queries.

---

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Integration](#integration)
- [Main Classes](#main-classes)
  - [`ListPagination`](#listpagination-class)
  - [`PaginationRenderer`](#paginationrenderer-class)
  - [`ListPaginationBinder`](#listpaginationbinder-class)
- [Core Methods & Properties](#core-methods--properties)
- [Extension Methods](#extension-methods)
- [Usage Examples](#usage-examples)
  - [Basic Pagination Usage](#basic-pagination-usage)
  - [Advanced Customization](#advanced-customization)
  - [Integration with ASP.NET MVC](#integration-with-aspnet-mvc)
- [Dependencies](#dependencies)
- [Exception Handling](#exception-handling)
- [Important Notes](#important-notes)

---

## Features

- **Easy pagination**: Straightforward implementation for lists, grids, and database queries.
- **Flexible configuration**: Supports customization of page descriptions, layout, navigation controls, and options.
- **AJAX integration**: Can seamlessly integrate with AJAX POST or GET requests for improved user experience.
- **Support MVC Model Binding**: Automatically binds pagination parameters from query strings and forms.
- **Integrated with Olive queries**: Works smoothly with Olive Entity Framework to paginate results directly from the database.
- **Dynamic Page sizes**: Supports selectable predefined page size options.

---

## Installation

Install via NuGet Package Manager in Visual Studio or CLI:

```powershell
Install-Package Olive.Mvc
```

This component depends on:

- Olive  
- Olive.Entities
- Microsoft.AspNetCore.Mvc
- Microsoft.AspNetCore.Mvc.ViewFeatures

Ensure these dependencies are also within your project requirements.

---

## Integration

To integrate pagination with your Olive-based MVC app:

### Step 1: Add pagination to your ViewModel

```csharp
public class PostListViewModel : IViewModel
{
    public ListPagination Paging { get; set; }
    public IEnumerable<Post> Posts { get; set; }
}
```

### Step 2: Apply page size and page numbering logic in your controller actions

```csharp
public async Task<IActionResult> List(PostListViewModel model)
{
    var query = Database.Of<Post>();

    model.Paging.TotalItemsCount = await query.Count();
    
    model.Posts = await query.Page(model.Paging).GetList();

    return View(model);
}
```

### Step 3: Display pagination controls in your Razor view

```razor
@using Olive.Mvc;
@model PostListViewModel

<div class="post-list">
    @foreach(var post in Model.Posts)
    {
        <div class="post-item">@post.Title</div>
    }
</div>

@Html.Pagination(Model.Paging)
```

---

## Main Classes

### `ListPagination` Class

Manages pagination parameters such as `PageSize`, `CurrentPage`, and total count of items.

Key properties include:

| Property | Description |
|----------|-------------|
| `CurrentPage` | Current active page number |
| `PageSize` | Number of items per page |
| `TotalItemsCount` | Total data set count |
| `Prefix` | Optional prefix to distinguish pagination parameters |
| `SizeOptions` | List of available page sizes |
| `LastPage` | Computed property for determining the last page number |  

### `PaginationRenderer` Class

Generates HTML for pagination controls. Integrates with Ajax and traditional navigation.

### `ListPaginationBinder` Class

Enables automatic binding of `ListPagination` from incoming query strings or form data in MVC actions.

---

## Core Methods & Properties

Methods of particular interest include:

```csharp
void AddPageSizeOptions(params object[] options)
PagingQueryOption ToQueryOption(string orderBy = null)
```

**Example Usage**:

```csharp
model.Paging.AddPageSizeOptions(10, 25, 50, 100);
var queryOption = model.Paging.ToQueryOption("DateCreated DESC");
```

---

## Extension Methods

Include extension methods to easily paginate query results:

- `TakePage` - For enumerables, applies page-based slicing to a collection.
- `Pagination` - For Razor views, renders pagination interface controls.
- `Page` - For Olive database queries, translates pagination details into Olive query options.

Example:

```csharp
var pagedResults = myEnumerable.TakePage(model.Paging);
```

or for Olive database queries:

```csharp
var items = await Database.Of<Post>().Page(model.Paging).GetList();
```

---

## Usage Examples

### Basic Pagination Usage

```csharp
// Controller
model.Paging.TotalItemsCount = await Database.Of<Post>().Count();
model.Posts = await Database.Of<Post>().Page(model.Paging).GetList();
```

```razor
// Razor View
@Html.Pagination(Model.Paging)
```

### Advanced Customization

Set custom navigation symbols or icons:

```csharp
ListPagination.DefaultFirstText = "<<";
ListPagination.DefaultPreviousText = "<";
ListPagination.DefaultNextText = ">";
ListPagination.DefaultLastText = ">>";
```

Enable advanced pagination controls such as First/Last or Previous/Next buttons:

```csharp
ListPagination.DefaultShowFirstLastLinks = true;
ListPagination.DefaultShowPreviousNextLinks = true;
```

### Integration with ASP.NET MVC

Easily integrate pagination with models via MVC model binding:

```csharp
public IActionResult Index(MyModel model)
{
    model.Paging.TotalItemsCount = // total items to paginate
    model.Items = myService.DoQuery(model.Paging);
    
    return View(model);
}
```

---

## Dependencies

Make sure the following packages are installed and referenced:

```powershell
Install-Package Olive.Mvc
Install-Package Olive.Entities
Install-Package Microsoft.AspNetCore.Mvc
Install-Package Microsoft.AspNetCore.Mvc.ViewFeatures
```

---

## Exception Handling

The pagination library handles typical exceptions gracefully by defaults to fallback values if invalid page numbers or sizes are requested.

Ensure you consider additional logic to handle exceptional situations or logging appropriately:

- Invalid or negative page requests default back to page 1.
- Invalid page sizes ignore the faulty size and optionally default to a fallback size or unpaged result.

---

## Important Notes

- Always ensure the pagination query logic considers sorting and indexing implications for optimal DB efficiency.
- Customize pagination appearance and behaviors through overriding CSS or changing default text.
- Confirm the integration and behavior of AJAX pagination control thoroughly before production usage.