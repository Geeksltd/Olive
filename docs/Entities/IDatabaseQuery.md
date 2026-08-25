
# IDatabaseQuery

In order to achieve a better performance in the Olive framework, you can benefit from IDatabaseQuery which lets you filter your dataset in the database layer.

## Sample
Let's refactor the following code to see how IDatabaseQueries work:

```csharp
async Task<IEnumerable<TaskItem>> GetPage(int pageIndex, int pageSize, DateTime? from = null, DateTime? to = null, bool excludeRejected = false)
{
    var excludeList = await Database.GetList<Comment>(c => c.Rejected).Select(c => c.TaskItem);
    
	//Retrieve all data from the database.
    var temp = await Database.GetList<TaskItem>();

	//Give up and some of the items based on the requested filter.
    if (from.HasValue)
        temp = temp.Where(t => t.DueDate > from);

    if (to.HasValue)
        temp = temp.Where(t => t.DueDate < to);

    if (excludeRejected)
        temp = temp.Except(excludeList);

	//Apply the paging.
    return temp.Skip(pageIndex * pageSize).Take(pageSize);
}
```

#### FYI
Yes, you can write the date conditions in a single line as below, but it is not as easy to read.
```csharp
var temp = await Database.GetList<TaskItem>(t => (from == null || t.DueDate > from) && (to == null || t.DueDate < to));
```
With the following code all the conditions and paging will apply at the database level.
```csharp
async Task<IEnumerable<TaskItem>> GetPage(int pageIndex, int pageSize, DateTime? from = null, DateTime? to = null, bool excludeRejected = false)
{
    var query = Database.Of<TaskItem>();

    if (from.HasValue)
        query.Where(t => t.DueDate > from);

    if (to.HasValue)
        query.Where(t => t.DueDate < to);

    if (excludeRejected)
    {
        var subQuery = Database.Of<Comment>().Where(c => c.Rejected);
        query.WhereNotIn(subQuery, c => c.TaskItem);
    }

    query.PageStartIndex = pageIndex * pageSize;
    query.PageSize = pageSize;

    return await query.GetList();
}
```

## Count()

`query.Count()` (and `Database.Count<T>(...)`) generate `SELECT Count(ID) FROM ...` rather than `SELECT Count(*) FROM ...`. Counting the primary key column lets SQL Server satisfy the query from the narrower primary key index instead of scanning/materializing every column of every matching row, which is cheaper on wide tables. The result is identical to `Count(*)`, since `ID` is a mandatory, non-null column on every entity.
