# Olive.Export

## Overview
The Olive Data Export module provides flexible utilities for exporting structured data into popular formats such as Excel XML and CSV. It includes robust customization mechanisms allowing fine-grained control over data content, column configuration, styling, and specialized functionalities like formulas, dropdowns (enumerations), hyperlinks, and grouped columns.

## Table of Contents
- [Key Components](#key-components)
- [Classes and Their Usage](#classes-and-their-usage)
  - [Exporter](#exporter)
  - [Exporter&lt;T&gt;](#exporter-t)
  - [Column&lt;T&gt;](#column-t)
  - [DropDownColumn&lt;T&gt;](#dropdowncolumn-t)
  - [Cell](#cell-class)
  - [CellStyle](#cellstyle-class)
- [Supported Export Formats](#supported-export-formats)
- [Usage Examples](#usage-examples)
- [Generating Excel XML](#generating-excel-xml)
- [Generating CSV](#generating-csv)
- [Advanced Styling Examples](#advanced-styling-examples)
- [Dependencies](#dependencies)
- [Exception Handling and Logging](#exception-handling-and-logging)
- [Important Notes](#important-notes)

---

## Key Components
- **Exporter classes** (`Exporter`, `Exporter<T>`) for initiating and managing the export process.
- **Column definitions** (`Column<T>`, `DropDownColumn<T>`) to structure and customize data fields.
- **Cells** (`Cell`) encapsulating data and their individual styling.
- **Styling system** (`CellStyle`) to control visual presentation including fonts, colors, alignment, and borders.
- **Multiple file formats** support (`ExcelXml`, `CSV`) for versatile data exchange.

---

## Classes and Their Usage

### Exporter
The base exporter class (`Exporter`) facilitates exporting arbitrary data:
- Can be directly initialized with a `System.Data.DataTable`.
- Provides methods and enums for alignment and orientation customization.

### Exporter\<T\>
Typed exporter for strongly-typed collections of objects:
- Manages document name, columns, rows, and sheet-level settings.
- Allows easy definition of custom columns and styling.

### Column\<T\>
Represents specific columns for export spreadsheets. Supports extensive customization:
- **HeaderText**: Column title.
- **DataType**: Data format (e.g., String, Number).
- **Style management**: Custom styles for headers/rows.
- **Formulas**: Excel formulas for automated calculations.
- **Grouping**: Supports grouped column headers.

### DropDownColumn\<T\>
Inherits from `Column<T>`. Provides Excel dropdown functionality:
- **PossibleValues** array for setting possible dropdown selections.
- **EnumerationName**: Named range in Excel to bind the cell dropdown.

### Cell Class
Represents individual spreadsheet cells holding data and styling.
- **Text and Type** attributes to define content and data type.
- Style customization using the embedded `CellStyle` object.

### CellStyle Class
Customization of cell appearance with granular settings such as alignment, fonts, colors, borders, rotations (`Vertical/Horizontal` orientations), background, etc.

---

## Supported Export Formats
- **Excel XML** (`.xls`): A versatile spreadsheet format compatible with Microsoft Excel.
- **Comma Separated Values (CSV)** (`.csv`): A simple text-based data format usable in many applications.

---

## Usage Examples

### Exporting Basic Data to Excel
```csharp
var exporter = new Exporter("BasicExport");
exporter.AddColumn("Name");
exporter.AddColumn("Age", "Number");
exporter.AddRow("Alice", 30);
exporter.AddRow("Bob", 25);
var document = exporter.ToDocument(Format.ExcelXml);
```

### Exporting a List with a Generic Exporter
```csharp
var items = new List<Employee> { employee1, employee2 };

var exporter = new Exporter<Employee>("Employees Report");
exporter.AddColumn("Name", "String", e => e.Name);
exporter.AddColumn("Hire Date", "DateTime", e => e.HireDate);

exporter.AddRows(items);
var result = exporter.Generate(Format.ExcelXml);
```

### DropDown Column Definition
```csharp
var exporter = new Exporter("DropdownFile");
exporter.AddDropDownColumn("Department", "String", "Departments", new[] {"IT","HR","Sales"});
exporter.AddRow("IT");
```

---

## Generating Excel XML
Supports advanced Excel features:
- Column freezing with `FreezeHeader` options.
- Cell merge functionality for header groupings.
- Links and formula support.
- Drop-down validations via enumerations.

### Example Excel XML generation
```csharp
var exporter = new Exporter("Report");
exporter.AddColumn("Name");
exporter.AddColumn("Salary", "Number").SetRowStyle(s => s.NumberFormat = "#,##0.00");
exporter.AddRow("John", 65000);
var xml = exporter.Generate(Format.ExcelXml);
```

---

## Generating CSV
```csharp
var exporter = new Exporter("SimpleCSV");
exporter.AddColumn("Product");
exporter.AddColumn("Price", "Number");
exporter.AddRow("Laptop", 1000);
exporter.AddRow("Phone", 800);
var csv = exporter.Generate(Format.Csv);
```

---

## Custom Styling with CellStyle
```csharp
var exporter = new Exporter("StyledDocument");
var column = exporter.AddColumn("Name");
column.HeaderStyle.FontSize = 14;
column.HeaderStyle.Bold = true;
column.SetRowStyle(s => {
    s.ForeColor = "#003366";
    s.BackgroundColor = "#EEEEEE";
});

exporter.AddRow("Chris");
exporter.AddRow("Alex");
```

---

## Dependencies
Install the following NuGet packages to leverage Olive Export features:

- Olive Framework:
```powershell
Install-Package Olive.Entities
```

- Olive Core Framework (`Olive`):
```powershell
Install-Package Olive
```

---

## Exception Handling and Logging
The exporter includes descriptive exceptions for troubleshooting:
- Validation errors (e.g., mismatched column counts).
- Invalid style settings (e.g., invalid border width).
- Errors in Excel formulas or CSV generation.

Detailed logging can be configured using Olive's built-in logging features, providing detailed insights into problematic export operations.

**Example**:
```
Exception: Invalid Link value for ExportToExcel: [provided invalid link string]
```

---

## Important Notes
- An Excel XML file generated with this exporter is XML-based and supports basic Excel features. Excel files generated as `.xls` from XML format might show a warning when opening in Excel; this is standard behavior due to Microsoft Excel's security model.
- Make sure sheet names used in one Excel workbook are unique.
- The current implementation handles standard string, numeric, and link data types. Extend carefully for additional complex scenarios.

---

## Dependencies
- `Olive.Entities` (core Olive data framework)
- `Olive` Core Extensions for additional utility methods

Include via NuGet:
```powershell
Install-Package Olive.Entities
Install-Package Olive
```

---

## Important Notes
- Remember to escape special CSV characters using the provided Olive extension methods.
- Excel XML output supports style-rich output, while CSV is limited to plain text.
- Avoid excessively large datasets in Excel XML, as this might lead to slower opening or increased memory usage. For large datasets, consider CSV exports.