# `DataTable` Extension Methods
>Provides some useful extension methods on DataTable objects, and you can work on the data table's records.

## ToCSV()
Gets the CSV data equivalent to this data table.
#### When to use it?
When you want to convert a datatable to a CSV format.
#### Example:

```csharp
SqlConnection conn = new SqlConnection(connectionString);
conn.Open();
string query = "SELECT * FROM [table1]";

SqlCommand cmd = new SqlCommand(query, conn);

DataTable dt = new DataTable();
dt.Load(cmd.ExecuteReader());
File.WriteAllText("test.csv", dt.ToCSV()); // creates an Excel format of "table1" table
```

## CastTo<T>()
Casts a data table's records into a list of typed objects.

#### When to use it?
When you want to cast a data table's records into a list of specific typed objects in your applications.

#### Example:

```csharp
SqlConnection conn = new SqlConnection(connectionString);
conn.Open();
string query = "SELECT * FROM [table1]";

SqlCommand cmd = new SqlCommand(query, conn);

DataTable dt = new DataTable();
dt.Load(cmd.ExecuteReader());
dt.CastTo<string>(); //returns this data table's records into a list of string objects
```

## CastTo<T>({propertyMappings})
Casts a data table's records into a list of typed objects. It can map datatable to a specific mapping.
>Note: propertyMappings is an anobymous object containing property mapping information.
>e.g.: new {Property1 = "Property name in CSV", Property2 = "...", set_Property1 = new Func<string>(text => Client.Parse(value)) }</param>

-Property convertors must start with 'set_{property name}'

#### When to use it?
When you want to cast a data table's records into a list of specific typed objects in your applications.
#### Example:

```csharp
SqlConnection conn = new SqlConnection(connectionString);
conn.Open();
string query = "SELECT * FROM [table1]";

SqlCommand cmd = new SqlCommand(query, conn);

DataTable dt = new DataTable();
dt.Load(cmd.ExecuteReader());
dt.CastTo<string>(
new {Property1 = "Property1 name in CSV", Property2 = "Property2 name in CSV", set_Property1 = new Func<string>(text => Client.Parse(value))
); //returns this data table's records into a list of string objects
```

