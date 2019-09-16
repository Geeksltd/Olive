
# Using C# Enums instead of M# Enums

### Pros and Cons:
There are different usages of these two types. So make sure you choose them wisely.

**Pros:**
- Better performance in certain scenarios
- More flexible database queries
- Smaller data size

**Cons:**
- It is easier to use the M# Enums.
- If there was no logic relying on the enums you won't need coding to add an item to your enum.

### Implementation
Let me demonstrate it with an example. 

As a use case, we want to implement an ordering scenario in which, each order needs a few steps. Therefore, we create an enum in our application.
```c#
public enum OrderStage
{
    New = 0,
    StockAvailabilityChecked = 1,
    ReadyForPayment = 2,
    Paid = 3,
    SentToProvider = 4,
    ProcessingByProvider = 5,
    Dispatched = 6,
    Delivered = 7
}
```
As each stage (value) has its own logic we cannot add a new stage by adding it to the database.

As the enums are stored as numeric values in the database we need to create an int property on the `Order` model and specify the C# type for it. Also, we need to let Olive knows how to convert the values. To do so, we need to add an attribute on the property.

```c#
Int("Stage")
    .CSharpTypeName("OrderStage")
    .Mandatory()
    .Attributes("[EnumDataConverter(\"Domain.OrderStage\")]");
```

Now we could store and query the orders as normal (Like using M# enums). But in addition to that, we could have queries using `<`, `<=`, `>` and `>=`. So you can get the list of orders up to a certain stage.

```c#
var list = await Database.GetList<Order>(o => o.Stage >= OrderStage.StockAvailabilityChecked && o.Stage < OrderStage.SentToProvider)
```

If you need the `in (...)` condition due to your logic. We need to take one more action. Simply create an extension method like below.
```c#
public static bool IsAnyOf(this OrderStage@this, params OrderStage[] values) 
=> values.Contains(@this);
```
Then you can have queries like this:
```c#
var list = await Database.GetList<Order>(o => o.Stage.IsAnyOf(OrderStage.StockAvailabilityChecked, OrderStage.SentToProvider));
```