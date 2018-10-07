# Olive.CSV

This librery helps you to work with CSV file as easy as possible. This library features reading CSV files by either **FileInfo**, **Blob** or raw CSV string.

## Reading CSV

Let's assume that we have a CSV containing following information:

![image](https://user-images.githubusercontent.com/22152065/37519997-9aaa7778-2930-11e8-89c7-1f5dafa35172.png)

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

You can read the columns of CSV content using following code:

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

## Writing CSV

Olive.CSV provides fluent extention methods to create CSV from **Dictionary**, **IEnumerable** and **DataTable**.
You can call `.ToCsv()` to get the CSV string. Also you can call `ToCsv().Save(new FileInfo("e:\\somewhere.csv"));` to save the CSV in your storage.

### Example

Here we want to create a CSV from a dictionary then save it somewhere then write it to console:

```csharp
//Create a name value dictionary
Dictionary<string, string> data = new Dictionary<string, string>();
data.Add("foo", "bar");
data.Add("x", "zee");
data.Add("somewhere", "not here");

//Save it somewhere
data.ToCsv().Save(new FileInfo("e:\\Somewhere.csv"));
//write the string value to console
Console.WriteLine(data.ToCsv());
```

Console output:

![image](https://user-images.githubusercontent.com/22152065/37524448-1b795e9c-293f-11e8-9694-c4bf2d3f95b4.png)

File output:

![image](https://user-images.githubusercontent.com/22152065/37524419-07fc1df0-293f-11e8-9b1e-b0d794d19ae5.png)
