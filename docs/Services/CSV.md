# Olive.CSV

This librery helps you to work with CSV file as easy as possible. This library features reading CSV files by either **FileInfo**, **Blob** or raw CSV string.

## Example

let's assume that we have a CSV containing following information:

![image](https://user-images.githubusercontent.com/22152065/37519997-9aaa7778-2930-11e8-89c7-1f5dafa35172.png)

Now let's get the information of this CSV via Olive.CSV library. First you need to create a test console application then install [Olive.CSV NuGet package](https://www.nuget.org/packages/Olive.Csv/).

`Install-Package Olive.CSV`

Now use this method to get the CSV file as a datatable:

```csharp
DataTable datatable = await CsvReader.ReadAsync(new FileInfo("E:\\Book1.csv"), false);
```

Also you can get the data table from a CSV by passing the raw CSV content directly:

```csharp
DataTable datatable = CsvReader.Read(@"
Name,Class,Grade
Alex,IT,30
Steven,Music,100
Aviv,Music,20
",
 false);
```

Now let's print out the second column of second row:

```csharp
Console.WriteLine(datatable.Rows[1][1]);
```

![image](https://user-images.githubusercontent.com/22152065/37521184-0842d196-2935-11e8-8d26-87de5dd6019c.png)

### Getting the columns as string array

You can read the columns of CSV content using follwing code:

```csharp
 var columns = CsvReader.GetColumnsAsync(new FileInfo("E:\\Book1.csv")).Result;
```

Notice that you can get columns either by passing the **FileInfo** to `CsvReader.GetColumnsAsync` or the raw CSV string to `CsvReader.GetColumns`.

Now let's print this one out:

```csharp
foreach (var column in columns)
{
    Console.WriteLine(column);
}
```

![image](https://user-images.githubusercontent.com/22152065/37521325-8c071528-2935-11e8-8259-cec2e94e0cfe.png)