# Generating Excel & CSV Files

In this doc we learn how to generate Excel and CSV files using Olive. Olive offers a concrete class called `ExcelExporter` for exporting *excel* and *CSV* files. We will see how easy it is in M# to export meaningful data.

## ExcelExporter Class

This class encapsulates Excel or CSV export functionality and has two constructors to instantiate the class, implementing a method `Generate`, which returns the formatted data, which is then dispatched in server response to export Excel or CSV file.

> **Note :** M# provides an extension method `Dispatch` on `HttpResponse` instance, which is used to send contents of different types in server response.

### ExcelExporter Output Option

ExcelExporter class provides an enum type `ExcelExporter.Output` which is used to determine the export format i.e. Excel or CSV

### Exporting Data using IEnumerable

The constructor shown below takes only one parameter to define the name of the *CSV or Excel File* being exported.

![image](https://user-images.githubusercontent.com/22152065/38164610-7adf87f6-351c-11e8-97d4-06ef91061e81.png)

This constructor is useful when you want to export data from any type of *object* other than a *DataTable*. Instantiating ExcelExporter using this constructor will require you to add the specified columns by calling `AddColumn` method on the `ExcelExporter` instance and then populating the data by calling `AddRow` method, as shown below:

```csharp
protected void ExportToExcel()
{
    var mode = ExcelExporter.Output.Csv;
    var mode = ExcelExporter("Products");

    //Add header texts
    exporter.AddColumn("Imported file");
    exporter.AddColumn("Title");
    exporter.AddColumn("Description");
    exporter.AddColumn("Condition");
    exporter.AddColumn("Price");

    //Add data rows
    foreach (var Item in Database.GetList<Product>())
    {
        exporter.AddRow(
            Item.ImportedFile,
            Item.Title ,
            Item.Description ,
            Item.Condition ,
            Item.Price
        )
    }

    Response.Dispatch(exporter.Generate(mode), "List: Product" + exporter.GetFileExtension(mode), "application/vnd.ms-excel",true,System.Text.Encoding.UTF8);
}
```

The code demonstrated above creates a new instance of `ExcelExporter`, creates the columns, populates the data, calls the `Generate` method of *exporter* and finally dispatches the returned String Object from `Generate` method with the file type and appropriate encoding. The *mode* variable defines the type of output file, which is *CSV*. In order to output Excel file select `Output.Excelxml` option.

### Exporting Data using DataTable

The second constructor requires a pre-populated `System.Data.DataTable` object. When using this constructor you do not need to define columns and populate rows explicitly as shown below:

![image](https://user-images.githubusercontent.com/22152065/38165035-f1833df2-3522-11e8-9bad-de2f79669463.png)

```csharp
protected void ExportTpExcel()
{
    var mode = ExcelExporter.Output.Csv;

    //Instantiate by assigning DataTable holding data
    var exporter = new ExcelExporter(GetDataTable());

    //Export the CSV
    Response.Dispatch(exporter.Generate(mode), "Products"+ exporter.GetFileExtension(mode), "application/vnd.ms-excel",true,System.Text.Encoding.UTF8);
}
```