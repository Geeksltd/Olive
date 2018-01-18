# Olive compatibility change log

## 18 Jan 2018
In Startup file in the Configure() method add:
```csharp
app.UseWebTest(config => config.AddEmail().AddTasks());
```
