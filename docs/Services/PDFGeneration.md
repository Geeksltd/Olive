# PDF Generation

This lesson focuses on the PDF generation option provided in Olive. In this lesson we will learn about exporting PDF documents and the basic configuration required.

Olive provides a static class `PdfService` under `Olive.PDF` namespace, which provides the functionality to generate PDF from HTML.

## Exporting PDF

In order to export a PDF document you must have a well formatted document with the relevant data or information. Olive uses HTML to PDF generation technique and makes it easier by allowing developers to use Webpages as the input of a PDF document.

In M# you simply need to develop a Webpage with the related data or information you want to export as PDF. Then, in order to export the PDF you just need the *Absolute URL* of that developed Webpage, as shown later in this lesson.

Olive allows developers to implement PDF Export and customization by defining the *PDF File Name and PDF customization* attribute on the *Navigate* action of any button. Olive *Navigate* action is used to redirect users to the pages developed in a website.

The code shown below demonstrates how to implement HTML to PDF export functionality:

```csharp
var url = Request.GetAbsoluteUrl(PageUrl("Login > Export Products"));

Response.Clear();
Response.ContentType = "Application/PDF";
Response.AddHeader("Content-Disposition","Attachment","filename=Products.pdf");

var pdfMaker = Olive.PDF.PdfService.CreateHtml2PdfConverter();

var pdfContent=pdfMaker.GetPdfFromUrlBytes(url);
ResponseBinaryWrite(pdfContent);
Response.End();
```

`GetAbsoluteURL` is an extension method on `HttpRequest` instance available in Olive, which returns absolute URL of the specified relative URL.

![image](https://user-images.githubusercontent.com/22152065/38164305-6c33bbd2-3517-11e8-8782-2bc4804cbe19.png)

`PageURL` method is implemented in the `UserControl` class in Olive, which is inherited by each and every module in Olive. This method accepts the *“resource key”* of the target page in sitemap and returns the relative URL of the page.

![image](https://user-images.githubusercontent.com/22152065/38164317-9f499654-3517-11e8-8248-b850528abcaf.png)

As shown in above, we need to define the *“ContentType”* and *“Header”* of the *“HttpResponse”* instance we are sending to the client browser so that the client browser can perform appropriate actions.

`PdfService.CreateHtml2PdfConverter()` creates an instance of Html 2 PDF converter service. `GetPdfFromUrlBytes` method returns the byte array object holding the given URL’s page html in bytes which is then sent to the client browser in server response.

`Html2PDFconverter` class exposes three main public properties to configure a PDF template

1) PdfHeaderOptions
2) PdfDocumentOptions
3) PdfFooterOptions

The above mentioned properties implement further properties to design each section e.g. Font, height, width, margins, quality etc.