# Profiling database access

In appSettings.js set `Profile=true`

### Step 1: Enable profiling

```json
"Database": {
    ...
    "Profile": false,
    ...
}
```

### Step 2: Add a dump command
Add a controller action in `SharedActionsController.cs` as the following:

```csharp
[Route("perf/dump")]
public async Task<IActionResult> DumpPerformance()
{
    var file = await Olive.Entities.Data.DataAccessProfiler.GenerateReport().ToCsvFile();
    return Content("Dump created: " + file.Name);
}
```
*Note: Your website project should have the nuget package `Olive.Export` installed.*

### Step 3: Collect data
Then run the app for a few minutes and test/invoke the scenarios you wish to measure. If you're not measuring a specific scenario, just play around with the app like a normal user would.
In this mode, the Olive data access API will measure every database query and add it to an in-memory log. This process is very fast and does not considerably impact your app performance. So it's safe to run on production server as well.
    
### Step 3: Generate a report
Once you're ready to get the performance data, in your browser address bar request `/perf/dump`.
At this point, a CSV file will be generated under your main website root, named `{timestamp}--Sql-Profiler.csv`.

The file will have the following columns generated for each unique sql command:

- Command (sql text)
- Calls (number of times executed)
- Total ms (sum of all executions)
- Longest ms
- Average ms
- Median ms (this is often more reliable than average)

---

### Profiling a specific action
Every time that you generate a dump, it will clear the in-memory log and start over.
This allows you to find out exactly what sql command are executed as a result of an action on the app UI. 
To do this:
1. Browser A: Run the app and navigate to the specific page or action that you wish to profile.
2. Browser B: Request `/perf/dump` to remove the logs related to any database action so far. You will ignore the generated CSV file.
3. Browser A: Invoke the action you wish to measure.
4. Browser B: Request `/perf/dump` again. The newly generated CSV file is what you want.
