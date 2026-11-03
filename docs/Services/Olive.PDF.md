# Olive.PDF

This document provides an overview and usage examples for the public class and interface in the `Olive.PDF` namespace. It facilitates the conversion of HTML content to PDF format using a configurable converter service. A configuration section specifies the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [PdfService](#pdfservice)
   - [Overview](#pdfservice-overview)
   - [Methods](#pdfservice-methods)
2. [IHtml2PdfConverter](#ihtml2pdfconverter)
   - [Overview](#ihtml2pdfconverter-overview)
   - [Methods](#ihtml2pdfconverter-methods)
3. [Configuration](#configuration)

---

## PdfService

### PdfService Overview

The `PdfService` static class provides a method to create an instance of an HTML-to-PDF converter service, based on a type specified in the configuration. It is designed to abstract the creation of PDF conversion services for use in Olive applications.

### PdfService Methods

- **`CreateHtml2PdfConverter()`**
  - Creates an instance of an `IHtml2PdfConverter` based on the configured converter type.
  - **Usage Example:**
    ```csharp
    // Assumes Olive.Html2Pdf:ConverterType is set in appsettings.json
    var converter = PdfService.CreateHtml2PdfConverter();
    byte[] pdfBytes = converter.GetPdfFromUrlBytes("https://example.com");
    File.WriteAllBytes("output.pdf", pdfBytes); // Saves the PDF to a file
    ```

---

## IHtml2PdfConverter

### IHtml2PdfConverter Overview

The `IHtml2PdfConverter` interface defines a contract for converting HTML content from a URL into a PDF byte array. Implementations of this interface are instantiated by `PdfService`.

### IHtml2PdfConverter Methods

- **`GetPdfFromUrlBytes(string url)`**
  - Converts the HTML content at the specified URL into a PDF byte array.
  - **Usage Example:**
    ```csharp
    // Example assuming an implementation is provided
    var converter = PdfService.CreateHtml2PdfConverter();
    byte[] pdfData = converter.GetPdfFromUrlBytes("https://example.com");
    Console.WriteLine($"PDF size: {pdfData.Length} bytes");
    ```

---

## Configuration

The `PdfService` class relies on a specific configuration setting stored in an `appsettings.json` file with a JSON structure. Below is the optional configuration:

- **`Olive.Html2Pdf:ConverterType`** (Optional)
  - Specifies the fully qualified type name of the `IHtml2PdfConverter` implementation to use. Defaults to `"Geeks.Html2PDF.Winnovative.Html2PdfConverter, Geeks.Html2PDF.Winnovative"` if not set.
  - **Example:**
    ```json
    {
      "Olive": {
        "Html2Pdf": {
          "ConverterType": "MyCustom.Html2PdfConverter, MyCustomAssembly"
        }
      }
    }
    ```

### Full `appsettings.json` Example
```json
{
  "Olive": {
    "Html2Pdf": {
      "ConverterType": "Geeks.Html2PDF.Winnovative.Html2PdfConverter, Geeks.Html2PDF.Winnovative"
    }
  }
}
```

### Notes
- If `Olive.Html2Pdf:ConverterType` is not specified, the default type (`Geeks.Html2PDF.Winnovative.Html2PdfConverter`) is used.
- The specified type must implement `IHtml2PdfConverter` and be accessible in the application's runtime environment. If the type cannot be loaded, an exception will be thrown with details about the failure.
- Ensure the assembly containing the converter type is referenced in your project.