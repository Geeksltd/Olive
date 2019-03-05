
# Linq Extension Methods
> `Linq` extension methods are really useful in all frameworks for working with IEnumerable objects.
> `Olive` has various methods which you can use in your applications.

## Add({dictionary})
Adds all items from a specified dictionary to `this` dictionary.
#### When to use it?
When you want to add all items from a specified dictionary to `this` dictionary in your applications.

#### Example:
```csharp
    var dictionary1 = new Dictionary<string, int>()
{
    {"mac", 1000},
    {"windows", 500}
};

    var dictionary2 = new Dictionary<string, int>()
{
    {"Sony", 1001},
    {"HTC", 501}
};

dictionary1.Add(dictionary2); // all items of dictionary2 added to dictionary1.
```

## Add({key},{value})
Adds the specified key/value pair to `this` list.
#### When to use it?
When you want to add the specified key/value pair to `this` list in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);
```

## AddFormat({format},{arguments})
Adds a item to a List with a specific format.
#### When to use it?
When you want to add a item to a List with a specific format in your applications.

#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("b");
list.AddFormat("How are {0} Mr. {1}?", "you","Farhad Abaei"); // returns {"aa","b","How are you Mr. Farhad Abaei?"}
```

## AddFormatLine({format},{arguments})
Adds a item to a List with a specific format and add `\r\n` to the end of this item.
#### When to use it?
When you want to add a item to a List with a specific format in your applications.
#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("b");
list.AddFormatLine("Hello"); // returns {"aa","b","Hello \r\n"}
```

## AddLine({text})
Adds the {test} and `\r\n` to the item and adds item to this List.
#### When to use it?
When you want to add the {test} and `\r\n` to the item and adds item to this List in your applications.
#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("b");
list.AddLine("Hello"); // returns {"aa","b","Hello \r\n"}
```
## AddFromProperties({data})
Adds the properties of a specified [anonymous] object as items to `this` dictionary.
It ignores duplicate entries and null values.
#### When to use it?
When you want to add the properties of a specified [anonymous] object as items to `this` dictionary in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

counts.AddFromProperties(100);
```
## AddRange({items})
Adds the specified items to `this` set. It works on `HashSet`.
`HashSet` is an optimized set collection. It helps eliminates duplicate strings or elements in an array. It is a set that hashes its contents.
#### When to use it?
When you want to add the specified items to `this` set in your applications.
#### Example:
```csharp
string[] array1 =
{
    "cat",
    "dog",
    "cat",
    "leopard",
    "tiger",
    "cat"
};
var hash = new HashSet<string>(array1); // returns {"cat","dog","leopard","tiger"}
IEnumerable<string> items = new string[] { "lion","horse","cat" };
hash.AddRange(items); // returns {"cat","dog","leopard","tiger","lion","horse"}
```

## AddRange({otherlist})
Adds all items of a list to `this` object.

#### When to use it?
When you want to add an item to `this` object in your applications.

#### Example:
```csharp
IList<string> templist = new List<string>();
templist.Add("1");
templist.Add("2");
templist.Add("5");
templist.Add("6");

string[] temp = new string[3];
temp[0] = "7";
temp[1] = "6";
temp[2] = "7";

templist.AddRange(temp);
```

## AllIndicesOf({item},{caseSensitive})
Gets all indices of the specified item in `this` collection.
#### When to use it?
When you want to get all indices of the specified item in `this` collection in your applications.
#### Example:
```csharp
IEnumerable<string> items2 = new string[] { "a","b","c","d","e","b","f","b" };
items33.AllIndicesOf("a"); //returns 0
items33.AllIndicesOf("c"); //returns 2
items33.AllIndicesOf("d"); //returns 3
items33.AllIndicesOf("b"); //returns {1,5,7}
items33.AllIndicesOf("g"); //returns no result
items33.AllIndicesOf("F",false); //returns 6
items33.AllIndicesOf("f",false); //returns 6
items33.AllIndicesOf("F",true); //returns no result
```
## Any({criteria})
Determines if `this` is `null` or an empty list
#### When to use it?
When you want to determine if `this` is `null` or an empty list in your applications.

#### Example:
```csharp
int[] array = { 1, 2, 3 };
array.Any(item => item % 2 == 0); // returns True. See if any elements are divisible by two.
array.Any(item => item > 3);// returns False. See if any elements are greater than three.
array.Any(item => item == 2); // returns True. See if any elements are 2.
array.Any(); // returns True. See if any elements are Null.
```

## AreItemsUnique({caseSensitive})
Determines if all items in `this` collection are unique.
#### When to use it?
When you want to determine if all items in `this` collection are unique in your applications.

#### Example:
```csharp
IList<string> items = new string[] { "a","b","c"};
items.AreItemsUnique(); // returns true
```
```csharp
IList<string> items = new string[] { "a","b","c","a"};
items.AreItemsUnique(); // returns false
```
```csharp
IList<string> items = new string[] { "a","b","c","A"};
items.AreItemsUnique(false); // returns false because caseSensitive is false and there are two 'a'
items.AreItemsUnique(true); // returns true because caseSensitive is true and there are two 'a'
```

```csharp
IList<string> items = new string[] {};
items.AreItemsUnique(); // returns true
```
## AverageOrDefault({selector})
Gets the average of the specified expression on all items of `this` list.
If the list is empty, null will be returned.
#### When to use it?
When you want to return the average of the specified expression on all items of `this` list in your applications.

#### Example:
```csharp
int[] intstr= { 1,2,3,4,5,6,7};
intstr.AverageOrDefault((f)=> f*2); // returns 8
intstr.AverageOrDefault((f)=> f); // returns 4
```
```csharp
int[] intstr= {};
intstr.AverageOrDefault((f)=> f); // returns null
```
## AwaitAll({task})
With `async` and `await` in C# we call functions in an asynchronous way. Like the rivers these tasks can run with no interference. Many things happen at once.
This extension method causes to all tasks are run as `await`.
#### When to use it?
When you want to run all tasks as `await` in your applications.
#### Example:
```csharp
public static Task Log(IAuditEvent auditEvent)
            => Context.Current.ServiceProvider.GetServices<IAuditLogger>().AwaitAll(x => x.Log(auditEvent));
```

## Cast({type})
Casts all items of `this` list to {type} lists.
#### When to use it?
When you want to cast all items of `this` list to {type} lists in your applications.
#### Example:
```csharp
IList<string> items = new string[] {"1","2","3","5","5","6"};
items.Cast(typeof(string)); // return an IEnumerable of {"1","2","3","5","5","6"}
```
## Chop({chopSize})
Chops a list into same-size smaller lists.
#### When to use it?
When you want to chop a list into same-size smaller lists in your applications.

#### Example:
```csharp
new int[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 }.Chop(5); // return: { {1,2,3,4,5}, {6,7,8,9,10}, {11,12,13,14,15}, {16} }
```

## Clone()
`Clone` is a full-featured language includes methods that may not be recommended. 
In C# (for example) the Clone method and `ICloneable` interface are available. But using these features causes confusion.
#### When to use it?
When you want to copy all items of `this` list to another list in your applications.
#### Example:
```csharp
IList<string> items = new string[] {"1","2","3","5","5","6"};
var items2=items.Clone(); // return an IEnumerable of {"1","2","3","5","5","6"}
```

## Concat({otherlist})
Concatenates all items of a list to `this` object.

#### When to use it?
When you want to concatenate an item to `this` object in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"item1"};
IEnumerable<string> items2 = new string[] { "item2", null};
items2= items2.Concat(items); // returns {"item1","item2", null}
```
## Concat({otherLists})
Concatenates all items of `this` list with the specified other lists
> It does not remove duplicated items automatically, and result items are not unique.
#### When to use it?
When you want to return the union of `this` list with the specified other lists in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] { "item1" };
IList<string> items2 = new string[] { "item1" ,"item2","items3"};

items.Concat(items2); // returns { "item1","item1","item2","items3". It is noticed that "item1" is duplicated in result list.
```

```csharp
IEnumerable<string> items = new string[] { };
IList<string> items2 = new string[] { };

items.Concat(items2); // returns empty list.
```

## Contains({instance},{caseSensitive})
Determines whether `this` list of strings contains the specified string.
#### When to use it?
When you want to determine whether `this` list of strings contains the specified `string` in your applications.

#### Example:
```csharp
string[] dtr = { "  str1", "str2   "," str3","str4   ","   str1    " };
dtr.Contains(" str3");// returns true
dtr.Contains(" Str3");// returns false
dtr.Contains(" tr3");// returns false
dtr.Contains(" Str3",false);// returns true
dtr.Contains(" Str3",true);// returns false
```

## Contains({item})
Determines if the specified item exists in `this` list.
#### When to use it?
When you want to determine if the specified item exists in `this` list in your applications.
#### Example:
```csharp
IList<string> items = new string[] {"a","b","c","d","e","f"};
items.Contains("c"); //  returns true
items.Contains("g"); //  returns false
```
## ContainsAll({Items},{caseSensitive})
Determines of `this` list contains all items of another given list.

#### When to use it?
When you want to determine of `this` list contains all items of another given list in your applications.

#### Example:
```csharp
int?[] intstr2 = { 10,20,30,40};
int?[] intstr1 = { 10, 20, 30, 40 };
intstr2.ContainsAll(aa); // returns true
```

```csharp
int?[] intstr1 = { 10, 20, 30, 40 };
int?[] intstr2 = { 10,20,30};
intstr1.ContainsAll(intstr2); //returns true 
```

```csharp
int?[] intstr1 = { 10, 20 };
int?[] intstr2 = { 10,20,30};
intstr1.ContainsAll(intstr2); //returns false
```

```csharp
int?[] intstr1 = { 10, 20 };
int?[] intstr2 = {};
intstr1.ContainsAll(intstr2); //returns true
```

```csharp
int?[] intstr1 = { 10, 20 };
int?[] intstr2 = {Null};
intstr1.ContainsAll(intstr2); //returns false
```

```csharp
int?[] intstr1 = {};
int?[] intstr2 = {};
intstr1.ContainsAll(intstr2); //returns true
```

```csharp
string[] intstr1 = { "a", "b" };
string[] intstr2 = {"A"};
intstr1.ContainsAll(intstr2,false); //returns true because caseSensitive is false, so "A" and "a" are equal
intstr1.ContainsAll(intstr2,true); //returns false because caseSensitive is true, so "A" and "a" are not equal
```


## ContainsAny({Items})
Determines if `this` list contains any of the specified items.

#### When to use it?
When you want to determine if `this` list contains any of the specified items in your applications.

#### Example:
```csharp
int?[] intstr1 = {};
int?[] intstr2 = {};
intstr1.ContainsAny(intstr2); //returns false
```

```csharp
int?[] intstr1 = {10,20,30,40,50};
int?[] intstr2 = { };
intstr1.ContainsAny(intstr2); //returns false
```

```csharp
int?[] intstr1 = {10,20,30,40,50};
int?[] intstr2 = { 10};
intstr1.ContainsAny(intstr2); //returns true
```

```csharp
int?[] intstr1 = {};
int?[] intstr2 = {10};
intstr1.ContainsAny(intstr2); //returns false
```

```csharp
int?[] intstr1 = {10};
int?[] intstr2 = {10};
intstr1.ContainsAny(intstr2); //returns true
```

## ContainsAtLeast({numberOfItems})
Determines whether `this` least contains at least the specified number of items.
This can be faster than calling "x.Count() >= N" for complex iterators.
#### When to use it?
When you want to determine whether `this` least contains at least the specified number of items in your applications.
#### Example:
```csharp
IEnumerable<string> items = new string[] { "a","b" };
list.ContainsAtLeast(0); // returns true
list.ContainsAtLeast(1); // returns true
list.ContainsAtLeast(2); // returns true
list.ContainsAtLeast(3); // returns false
list.ContainsAtLeast(-1); // throws error : "The numberOfItems should be greater than or equal to 0."
```

## Count({criteria})
Counts the number of items in `this` list matching the specified criteria.
{criteria} is a Func() which determines the condition.
#### When to use it?
When you want to count the number of items in `this` list matching the specified criteria in your applications.
#### Example:
```csharp
int?[] intstr1 = { 1,2,1,6 };
intstr.Count((xa) => xa==1); // returns 2

int?[] intstr = {};
intstr.Count((xa) => xa==1); // returns 0
```
## DequeueAll()
Dequeues all queued items in the right order.
#### When to use it?
When you want to dequeue all queued items in the right order in your applications.
#### Example:
```csharp
Queue myQ = new Queue();
myQ.Enqueue("The");
myQ.Enqueue("quick");
myQ.Enqueue("brown");
myQ.Enqueue("fox");

var deque=myQ.DequeueAll(); // returns {"The","quick","brown","fox"}
```

## Distinct({selector},{caseSensitive})
Returns distinct elements from a sequence by using the default equality comparer to compare values.

#### When to use it?
When you want to return distinct elements from a sequence in your applications.

#### Example:
```csharp
int?[] intstr2 = { 10,20,30,40,50,10,10,20};
intstr2.Distinct(); // returns {10,20,30,40,50}
```
```csharp
string[] intstr2 = { "a","b","c","d","A","B","a","B"};
intstr2.Distinct(false); // returns {"a","b","c","d"}
intstr2.Distinct(true); // returns {"a","b","c","d"","A","B"}
```



## Except({item},{criteria},{caseSensitive})
Gets all items of `this` list except those meeting a specified criteria.
{criteria} is Exclusion criteria.
#### When to use it?
When you want to get all items of `this` list except those meeting a specified criteria in your applications.

#### Example:
```csharp
AppDomain.CurrentDomain.GetAssemblies().Except(t => t.Name.StartsWith("App_Code"));

IEnumerable<string> items = new string[] {"msg","item1","item2" }.Concat("new string");
IEnumerable<string> items2 ;
items2= items.Except("msg"); // "msg" is removed. item2 has not "msg" item.
items2= items.Except("msg","item1"); // "msg" and "item1" are removed. item2 has not "msg" and "item1" items.

var list2 = new List<string>();
list.Add("item1");
list.Add("item2");
items2= items.Except(list); // "item1" and "item2" are removed. item2 has not "item1" and "item2" items.

var list3 = new List<string>();
var list4 = new List<string>();
list.Add("ITEM1");
list.Add("ITEM2");
items3= items.Except(list,false); // "item1" and "item2" are removed. item2 has not "item1" and "item2" items.
items4= items.Except(list,true); // "item1" and "item2" are not removed.
```

## Except({item},{criteria},{alsoDistinct})
Gets all items of `this` list except those meeting a specified criteria. 
{criteria} is exclusion criteria.
if {alsoDistinct} is `true`, it returns distinct elements from a sequence. The default value is `false`.

#### When to use it?
When you want to get all items of `this` list except those meeting a specified criteria in your applications.

#### Example:
```csharp
AppDomain.CurrentDomain.GetAssemblies().Except(t => t.Name.StartsWith("App_Code"));

IEnumerable<string> items = new string[] {"msg","item1","item2" }.Concat("new string");
IEnumerable<string> items2 ;
items2= items.Except("msg"); // "msg" is removed. item2 has not "msg" item.
items2= items.Except("msg","item1"); // "msg" and "item1" are removed. item2 has not "msg" and "item1" items.

var list2 = new List<string>();
list.Add("item1");
list.Add("item2");
items2= items.Except(list); // "item1" and "item2" are removed. item2 has not "item1" and "item2" items.
```

## ExceptNull()
Gets all `Non-NULL` items of `this` list.

#### When to use it?
When you want to get all `Non-NULL` items of `this` list in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"msg","item1","item2",null};
items2= items.ExceptNull(); // null is removed. item2 has not null item.
```

## ExceptAt({indices})
Returns all elements of `this` list except those at the specified indices.
#### When to use it?
When you want to return all elements of `this` list except those at the specified indices in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

counts.ExceptAt(2,3,8); // returns {0,1,4}
```

## ExceptLast({count})
Returns all elements of `this` list except the last X items.
{count} determines the number of excepted items. The default value is 1.
#### When to use it?
When you want to return all elements of `this` list except the last X items in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

counts.ExceptLast(); // returns {0,1,2,3}
counts.ExceptLast(1); // returns {0,1,2,3}
counts.ExceptLast(2); // returns {0,1,2}
counts.ExceptLast(5); // returns {}
counts.ExceptLast(50); // returns {}
counts.ExceptLast(0); // returns {0,1,2,3,4}
counts.ExceptLast(-1); // throws an exception
```

## ExceptFirst({count})
Returns all elements of `this` list except the first X items.
{count} determines the number of excepted items. The default value is 1.
#### When to use it?
When you want to return all elements of `this` list except the first X items in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

counts.ExceptFirst(); // returns {1,2,3,4}
counts.ExceptFirst(1); // returns {1,2,3,4}
counts.ExceptFirst(2); // returns {2,3,4}
counts.ExceptFirst(5); // returns {}
counts.ExceptFirst(50); // returns {}
counts.ExceptFirst(0); // returns {0,1,2,3,4}
counts.ExceptFirst(-1); // throws an exception
```

## FirstOrDefault({selector})
`FirstOrDefault` is almost the same as `First`. The difference is how it handles empty collections. 
If a collection is `empty`, it returns the default value for the type; otherwise, it returns {selector} output.
The method internally checks if an element exists.
#### When to use it?
When you want to return the default value for the type if a collection is `empty` in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

counts.FirstOrDefault((qa)=> qa.Value==2); //returns "key2"
counts.FirstOrDefault((qa)=> qa.Value==2); //returns null
```

## GetAllValues()
Gets all values from `this` dictionary or IEnumerable.
#### When to use it?
When you want to get all values from `this` dictionary in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);
counts.GetAllValues(); // Returns {0,1,2,3,4}
```

## GetElementAfter({item},{caseSensitive})
Gets the element after a specified item in `this` list.
If the specified element does not exist in `this` list, an ArgumentException will be thrown.
If the specified element is the last in the list, `NULL` will be returned.
#### When to use it?
When you want to get the element after a specified item in `this` list in your applications.

#### Example:
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("bb");
list.Add("cc");
list.GetElementAfter("aa"); // returns "bb"
list.GetElementAfter("bb"); // returns "cc"
list.GetElementAfter("cc"); // returns Null
list.GetElementAfter("dd"); // throws an exception :"The specified item does not exist to this list."


list.GetElementAfter("BB",false); // returns "cc", because caseSensitive is false
list.GetElementAfter("BB",true); // throws an exception :"The specified item does not exist to this list.", because caseSensitive is true

list.GetElementAfter("AA",false); // returns null
list.GetElementAfter("AA",true); // throws an exception :"The specified item does not exist to this list.", because caseSensitive is true

```

## GetElementBefore({item},{caseSensitive})
Gets the element before a specified item in `this` list.
If the specified element does not exist in `this` list, an ArgumentException will be thrown.
If the specified element is the first in the list, `NULL` will be returned.
#### When to use it?
When you want to get the element after a specified item in `this` list in your applications.

#### Example:
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("bb");
list.Add("cc");
list.GetElementBefore("aa"); // returns Null
list.GetElementBefore("bb"); // returns "aa"
list.GetElementBefore("cc"); // returns "bb"
list.GetElementBefore("dd"); // throws an exception :"The specified item does not exist to this list."

list.GetElementBefore("BB",false); // returns "aa", because caseSensitive is false
list.GetElementBefore("BB",true); // throws an exception :"The specified item does not exist to this list.", because caseSensitive is true

list.GetElementAfter("CC",false); // returns null
list.GetElementAfter("CC",true); // throws an exception :"The specified item does not exist to this list.", because caseSensitive is true

```

## GetKeys()
Gets the keys of this dictionary.
#### When to use it?
When you want to get the keys of `this` dictionary in your applications.
This method optimizes `Dictionary` usage. It gets a value (at a key) from a Dictionary. And it eliminates unneeded lookups, making programs better.
Some notes. With `TryGet`, we can combine the "try" part of seeing if a key exists, and the "get" part of getting the existing value. This saves 1 lookup.
We can rewrite programs that use ContainsKey` with `TryGet`. By combining two operations (try and get) into 1, we can optimize and simplify programs.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);
counts.GetKeys(); // returns {"key0","key1","key2","key3","key4"}
```

## IndexOf({keyString},{Criteria Func},{caseSensitive})
Determines whether `this` value has {keyString} or not. If it has, this method returns its index. Otherwise, it returns -1.
Another override gets a `Func<>` which determines the criteria of index of items.
#### When to use it?
When you want to determine whether a list has a key or not in your applications.

#### Example:
```csharp
string[] dtr = { "str1", "str2","str3","str4" };
dtr.IndexOf("str1"); //returns 0
dtr.IndexOf("str2"); //returns 1
dtr.IndexOf("str3"); //returns 2
dtr.IndexOf("str4"); //returns 3
dtr.IndexOf("str5"); //returns -1
dtr.IndexOf("STR4",false); //returns 3, because caseSensitive is false
dtr.IndexOf("STR4",true); //returns -1, because caseSensitive is True


dtr.IndexOf((tr) => tr=="str"+"3"); //returns 2
dtr.IndexOf((tr) => tr=="str"+"1"); //returns 0
dtr.IndexOf((tr) => tr=="str"+"6"); //returns -1
```

## Intersects({otherList},{caseSensitive})
Determines if `this` list intersects with another specified list.
#### When to use it?
When you want to determine if `this` list intersects with another specified list in your applications.

#### Example:
```csharp
// Assign two arrays.
int[] array1 = { 1, 2, 3 };
int[] array2 = { 2, 3, 4 };
int[] array3 = { 10,20,30 };
// Call Intersect extension method.
array1.Intersects(array2); // returns true
var intersect2 = array1.Intersects(array3); // returns false
```
```csharp
int[] array1 = { 7,5,6};
int[] array2 = { };
array1.Intersects(array2); // returns false
```

```csharp
string[] array1 = { "a", "b", "c" };
string[] array2 = { "a", "B", "c" };
array1.Intersects(array2,false); // returns true
array1.Intersects(array2,true); // returns false, because caseSensitive is true, so "b" and "B" are different.
var intersect2 = array1.Intersects(array3); // returns false
```



## IsAnyOf({items},{caseSensitive})
Determines if `this` item is in the specified list.
#### When to use it?
When you want to determine if `this` item is in the specified list in your applications.
#### Example:
```csharp
IList<string> items = new string[] { "b" ,"aa","c","aa","h","aa"};
"aa".IsAnyOf(items); // returns true
"aaa".IsAnyOf(items);// returns false 

"AA".IsAnyOf(items,false);// returns True, because caseSensitive is false
"AA".IsAnyOf(items,true);// returns False, because caseSensitive is true, so "AA" and "aa" are different.
```
## IsEquivalentTo({targetlist},{caseSensitive})
Determines whether `this` list is equivalent to another specified list. Items in the list should be distinct for accurate result.
#### When to use it?
When you want to whether `this` list is equivalent to another specified list in your applications.

#### Example:
```csharp
IList<int?>  targetList = new List<int?>();
targetList.Add(1);
targetList.Add(2);
targetList.Add(5);
targetList.Add(6);

int?[] sourceList = { 1,2 };
sourceList.IsEquivalentTo(targetList); // returns false

int?[] sourceList2 = { 1,2,5,6 };
sourceList2.IsEquivalentTo(targetList); // returns true
```

```csharp
IList<string>  targetList = new List<string>();
targetList.Add("a");
targetList.Add("b");
targetList.Add("c");
targetList.Add("d");

string[] sourceList = { "A","B","C","D" };
sourceList.IsEquivalentTo(targetList,false); // returns true
sourceList.IsEquivalentTo(targetList,true); // returns false
```



## IsSingle()
Determines whether `this` value has one item or not. If `this` value has one item, it returns true. Otherwise, it returns false.

#### When to use it?
When you want to determine whether a list has one item or not in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"msg","item1","item2"};
items.IsSingle();// returns false

IEnumerable<string> items = new string[] {};
items.IsSingle();// returns false

IEnumerable<string> items = new string[] {null};
items.IsSingle();// returns true

IEnumerable<string> items = new string[] {null,null};
items.IsSingle();// returns false
```

## IsSubsetOf({targetlist},{caseSensitive})
Determines whether a list contains all members of another list.
#### When to use it?
When you want to determine whether a list contains all members of another list in your applications.

#### Example:
```csharp
IList<int?>  targetList = new List<int?>();
targetList.Add(1);
targetList.Add(2);
targetList.Add(5);
targetList.Add(6);

int?[] sourceList = { 1,2 };
sourceList.IsSubsetOf(targetList); // returns true
```
```csharp
IList<string>  targetList = new List<string>();
targetList.Add("a");
targetList.Add("b");
targetList.Add("c");
targetList.Add("d");

string[] sourceList = { "A","B" };
sourceList.IsSubsetOf(targetList, false); // returns true because caseSensitive is false, so "A" and "a" are equal
sourceList.IsSubsetOf(targetList, true); // returns false because caseSensitive is true, so "A" and "a" are different
```

## Lacks({item})
Works as opposite of Contains().

#### When to use it?
When you want to determine whether `this` list object does not contain the specified item in your applications.
#### Example:
```csharp
int?[] intstr2 = { 1,2,1,6 };
intstr2.Lacks(6); //returns false
intstr2.Lacks(33); //returns true
intstr2.Lacks(7); //returns true
intstr2.Lacks(3); //returns true
intstr2.Lacks(1); //returns false
intstr2.Lacks(-2); //returns true
```

## Lacks({keyString})
Determines whether a dictionary has {keyString} or not. If it has, this method returns false. Otherwise, it returns true.
#### When to use it?
When you want to determine whether a dictionary has a key or not in your applications.

#### Example:
```csharp
var dictionary = new Dictionary<string, int>()
{
    {"mac", 1000},
    {"windows", 500}
};

dictionary.Lacks("windows"); // returns false
dictionary.Lacks("window")' // returns true
```

## Lacks({instance},{caseSensitive})
Determines whether `this` list of strings contains the specified string. It is opposite of `Contains()` function.
#### When to use it?
When you want to determine whether `this` list of strings contains the specified `string` in your applications.

#### Example:
```csharp
string[] dtr = { "  str1", "str2   "," str3","str4   ","   str1    " };
dtr.Lacks(" str3");// returns false
dtr.Lacks(" Str3");// returns true
dtr.Lacks(" tr3");// returns true
dtr.Lacks(" Str3",false);// returns false
dtr.Lacks(" Str3",true);// returns true
```
## Lacks({item})
Determines if the specified item exists in `this` list. 
#### When to use it?
When you want to determine if the specified item exists in `this` list in your applications.
#### Example:
```csharp
IList<string> items = new string[] {"a","b","c","d","e","f"};
items.Lacks("c"); //  returns false
items.Lacks("g"); //  returns true
```
## LacksAny({item},{caseSensitive})
Determines if `this` list lacks any item in the specified list.

#### When to use it?
When you want to determine if `this` list lacks any item in the specified list in your applications.

#### Example:
```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);
int?[] intstr2 = { 1,2,3,6,7 };
intstr2.LacksAny(aa); // returns false
```
```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);

int?[] intstr2 = { 1,2,3};
intstr2.LacksAny(aa); // returns false
```

```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);
aa.Add(4);

int?[] intstr2 = {1,2,3};
intstr2.LacksAny(aa); // returns true
```

```csharp
IList<string> aa = new List<string>();
aa.Add("a");
aa.Add("b");
aa.Add("c");

string[] intstr2 = {"A","B","C"};
intstr2.LacksAny(aa,false); // returns false because caseSensitive is false, so "a" and "A" are different
intstr2.LacksAny(aa,true); // returns true
```



## LacksAll({item},{caseSensitive})
Determines if `this` list lacks all items in the specified list.
If all members of two lists are completely different, it returns `true`; otherwise, `false`.

#### When to use it?
When you want to determine if `this` list lacks any item in the specified list in your applications.

#### Example:
```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);
int?[] intstr2 = { 1,2,3,6,7 };
intstr2.LacksAny(aa); // returns false
```

```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);

int?[] intstr2 = { 1,2,3};
intstr2.LacksAll(aa); // returns false
```

```csharp
IList<int?> aa = new List<int?>();
aa.Add(1);
aa.Add(2);
aa.Add(3);
aa.Add(4);

int?[] intstr2 = {1,2,3};
intstr2.LacksAll(aa); // returns false
```

```csharp
IList<int?> aa = new List<int?>();
aa.Add(10);
aa.Add(20);
aa.Add(30);

int?[] intstr2 = {1,2,3};
intstr2.LacksAll(aa); // returns true
```
```csharp
IList<string> aa = new List<string>();
aa.Add("a");
aa.Add("b");

string[] intstr2 = {"A","B"};
intstr2.LacksAll(aa,true); // returns true because caseSensitive is true, so "A" and "a" are different
intstr2.LacksAll(aa,false); // returns false 
```




## LacksKey({keyString})
Determines whether a dictionary has {keyString} or not. If it has, this method returns `false`. Otherwise, it returns `true`.
#### When to use it?
When you want to determine whether a `dictionary` has a key or not in your applications.

#### Example:
```csharp
var dictionary = new Dictionary<string, int>()
{
    {"mac", 1000},
    {"windows", 500}
};

dictionary.LacksKey("windows"); // returns false
dictionary.LacksKey("window")' // returns true
```
## MaxOrDefault({criteria})
Gets the maximum value of a specified expression in `this` list. If the list is `empty`, then the default value of the expression will be returned.
{criteria} is a `Func()` which determines the condition.
#### When to use it?
When you want to get the maximum value of a specified expression in this list in your applications.

#### Example:
```csharp
IList<string> dtr2 = new string[] { "str5", "str2", "str3", "str1" };
dtr2.MinOrDefault(fx => fx.ToUpper()); // returns "STR5"

IList<string> dtr2 = new string[] { "5", "4", "3", "2" };
dtr2.MinOrDefault(fx => fx= fx+10); // returns "510"
```

## MaxOrNull({criteria})
Gets the maximum value of the specified expression in `this` list. 
If no items exist in the list then `null` will be returned. 
{criteria} is a `Func()` which determines the condition.
#### When to use it?
When you want to get the maximum value of the specified expression in `this` list in your applications.

#### Example:
```csharp
int?[] intstr = { 1, 2, 3, 4, 5, 6 };
intstr.MaxOrNull(fx => fx= fx+10); // returns 16

```
## Median()
Finds the median of a list of integers.
> The list cannot be `empty`.
#### When to use it?
When you want to find the median of a list of integers in your applications.
#### Example:
```csharp
IEnumerable<int> Numberitems = new int[] {10,20,30,40,50,60};
Numberitems.Median() // returns 30
```
```csharp
IEnumerable<int> Numberitems = new int[] {10,20,30,40,50,60,70};
Numberitems.Median() // returns 35
```
```csharp
IEnumerable<int> Numberitems = new int[] {-10,20,30,40,50,60,70};
Numberitems.Median() // returns 35
```
```csharp
IEnumerable<int> Numberitems = new int[] {-10,-20,-30,40,50,60};
Numberitems.Median() // returns 25
```
```csharp
IEnumerable<int> Numberitems = new int[] {-10,-20,-30,40,50,60,70};
Numberitems.Median() // returns 40
```
```csharp
IEnumerable<int> Numberitems = new int[] {};
Numberitems.Median() // throws an exception: "number list cannot be empty"
```
## MinOrDefault({criteria})
Gets the minimum value of a specified expression in `this` list. If the list is `empty`, then the default value of the expression will be returned.
{criteria} is a `Func()` which determines the condition.
#### When to use it?
When you want to get the minimum value of a specified expression in `this` list in your applications.

#### Example:
```csharp
IList<string> dtr2 = new string[] { "str5", "str2", "str3", "str1" };
dtr2.MinOrDefault(fx => fx.ToUpper()); // returns "STR1"

IList<string> dtr2 = new string[] { "5", "4", "3", "2" };
dtr2.MinOrDefault(fx => fx= fx+10); // returns "210"
```
## MinOrNull({criteria})
Gets the minimum value of the specified expression in `this` list. 
If no items exist in the list then `null` will be returned. 
{criteria} is a `Func()` which determines the condition.
#### When to use it?
When you want to get the minimum value of the specified expression in `this` list in your applications.

#### Example:
```csharp
int?[] intstr = { 1, 2, 3, 4, 5, 6 };
intstr.MinOrNull(fx => fx= fx+10); // returns 11
```
## None({criteria})
Determines if this is `null` or an `empty` list.
#### When to use it?
When you want to determine if `this` is `null` or an `empty` list in your applications.

#### Example:
```csharp
int[] array = { 1, 2, 3 };
array.None(item => item % 2 == 0); // returns False.
array.None(item => item > 3);// returns True. 
array.None(item => item == 2); // returns False.
array.None(); // returns false.
```

```csharp
int?[] array2 = { 1, 2, 3,null };
array2.None(); // returns false.
```

```csharp
int?[] array2 = { };
array2.None();// returns true.
```
## Or({valueIfEmpty})
If `this` list is `null` or `empty`, then the specified alternative will be returned, otherwise `this` will be returned.

#### When to use it?
When you want to find a specified alternative if `this` list is `null` or `empty` in your applications.
#### Example:
```csharp
IEnumerable<int?> Numberitems = new int?[] {};
IEnumerable<int?> temp= new int?[] { 10, 20, 30, 40, 50 };
Numberitems= Numberitems.Or(temp); // returns { 10, 20, 30, 40, 50 }
```
```csharp
IEnumerable<int?> Numberitems = new int?[] {null};
IEnumerable<int?> temp= new int?[] { 10, 20, 30, 40, 50 };
Numberitems= Numberitems.Or(temp); // returns { null }
```

## OrderBy({propertyName})
Sorts `this` list by the specified property name. 
It sorts the items in ascending order by default. To sort the items in descending order, use the OrderByDescending().
#### When to use it?
When you want to sort `this` list by the specified property name in your applications.
#### Example:
```csharp
IEnumerable<int> temp= new int[] {5,4,7,8,1,4,0};
Numberitems= Numberitems.Or(temp);
var temp2 = temp.OrderBy((asd)=> asd.ToString()); // returns {0,1,4,4,5,7,8}
```
```csharp
IEnumerable<string> temp= new string[] {"a","aaaa","aa","aaa","aaaa","aa","aaaaa"};
var temp2 = temp.OrderBy((asd)=> asd.Length.ToString());  //returns {"a","aa","aa","aaa","aaaa","aaaaa"}
```
```csharp
IEnumerable<string> temp= new string[] {"a","aaaa","aa","aaa","aaaa","aa","aaaaa"};
var temp2 = temp.OrderBy(null);  // throws an error
```

## OrderByDescending({propertyName})
Sorts `this` list by the specified property name. 
It sorts the items in descending order by default. To sort the items in ascending order, use the OrderBy().
#### When to use it?
When you want to sort `this` list by the specified property name in your applications.
#### Example:
```csharp
IEnumerable<int> temp= new int[] {5,4,7,8,1,4,0};
Numberitems= Numberitems.Or(temp);
var temp2 = temp.OrderByDescending((asd)=> asd.ToString()); // returns {8,7,5,4,4,1,0}
```
```csharp
IEnumerable<string> temp= new string[] {"a","aaaa","aa","aaa","aaaa","aa","aaaaa"};
var temp2 = temp.OrderByDescending((asd)=> asd.Length.ToString());  //returns {"aaaaa","aaaa","aaa","aa","a","a"}
```
```csharp
IEnumerable<string> temp= new string[] {"a","aaaa","aa","aaa","aaaa","aa","aaaaa"};
var temp2 = temp.OrderByDescending(null);  // throws an error
```

## OrEmpty()
Returns an empty collection if `this` collection is null.
#### When to use it?
When you want to return an `empty` collection if `this` collection is `null` in your applications.
#### Example:
```csharp
IEnumerable<string> items = new string[] {};
items.OrEmpty(); //returns empty collection
```
```csharp
IEnumerable<string> items = new string[] {null};
items.OrEmpty(); //returns {null}
```
```csharp
IEnumerable<string> items = new string[] {"1","2"};
items.OrEmpty(); //returns {"1","2"}
```

## PadLeft({size},{padItemValue})
Add {padItemValue} to the left side of this list if the size parameter is greater than the lenght of the list.
{size} is the number of items.
{padItemValue} is the string should be added to the left side of the list.
#### When to use it?
When you want to fix the number of items of a list and fill missed items with a specific string in left side in your applications.

#### Example:
```csharp
string[] intstr = { "1", "2", "3", "4", "5", "6", "7" };
var newStr = intstr.PadLeft<string>(10,"abc"); //returns {"abc", "abc", "abc", "1", "2", "3", "4", "5", "6", "7" }  newStr has 10 items.
```
```csharp
string[] intstr = { "1", "2", "3", "4", "5", "6", "7" };
var newStr = intstr.PadLeft<string>(5,"abc"); //returns {"1", "2", "3", "4", "5", "6", "7" }
```
```csharp
string[] intstr = {};
var newStr = intstr.PadLeft<string>(5,"abc"); //returns {}
```
```csharp
string[] intstr = {null};
var newStr = intstr.PadLeft<string>(5,"abc"); //returns {"abc","abc","abc","abc",null}
```

## PadRight({size},{padItemValue})
Add {padItemValue} to the right side of this list if the size parameter is greater than the lenght of the list.
{size} is the number of items.
{padItemValue} is the string should be added to the right side of the list.

#### When to use it?
When you want to fix the number of items of a list and fill missed items with a specific string in right side in your applications.

#### Example:
```csharp
string[] intstr = { "1", "2", "3", "4", "5", "6", "7" };
var newStr = intstr.PadRight<string>(10,"abc"); //returns {"1", "2", "3", "4", "5", "6", "7", "abc", "abc", "abc" }  newStr has 10 items.
```
```csharp
string[] intstr = { "1", "2", "3", "4", "5", "6", "7" };
var newStr = intstr.PadRight<string>(5,"abc"); //returns {"1", "2", "3", "4", "5", "6", "7" }
```
```csharp
string[] intstr = {};
var newStr = intstr.PadRight<string>(5,"abc"); //returns {}
```
```csharp
string[] intstr = {null};
var newStr = intstr.PadRight<string>(5,"abc"); //returns {null, "abc","abc","abc","abc"}
```

## PickRandom({Number})
Picks an item from the list. {number} is the number of items which are selected randomly. The default value is 1 and it should be greater than 0.

#### When to use it?
When you want to pick an item from the list in your applications.

#### Example:
```csharp
int?[] intstr1 = { 1,2,1,6 };
intstr.PickRandom(); // returns a random number from the list.

intstr.PickRandom(3); // returns a 3 random numbers from the list.

intstr.PickRandom(0); // throws "number should be greater than 0." error.
```
## Prepend({otherlist})
Adds the specified list to the beginning of `this` list.

#### When to use it?
When you want to add the specified list to the beginning of `this` list in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"ggg"};
IEnumerable<string> items2 = new string[] { "abc" };
IEnumerable<string> itemsResult = new string[] {};

itemsResult = items2.Prepend(items); // returns "ggg" and "abc"
```
## Randomize()
Picks an item from the list.

#### When to use it?
When you want to pick an item from the list in your applications.

#### Example:
```csharp
int?[] intstr = { 1,2,1,6 };
intstr.Randomize(); // returns a random number from the list, for example 2.
```

## Remove({itemsToRemove},{caseSensitive})
Removes a list of items from this list.
#### When to use it?
When you want to remove a list of items from this list in your applications.

#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("c");
list.Add("b");
IList<string> items = new string[] { "b" ,"aa"};
list.Remove(items); // returns {"c"}
```

```csharp
var list = new List<String>();
list.Add("aa");
list.Add("c");
list.Add("b");
IList<string> items = new string[] { "b" ,"AA"};
list.Remove(items, false); // returns {"c"}
list.Remove(items, true); // returns {"aa","c"} because caseSensitive is true, so "AA" and "aa" are different
```

## RemoveNulls()
Removes the nulls from `this` list.
#### When to use it?
When you want to remove the nulls from `this` list in your applications.
#### Example:
```csharp
IList<int?> aa = new List<int?>();
aa.Add(10);
aa.Add(20);
aa.Add(30);
aa.Add(40);
aa.Add(null);

aa.RemoveNulls(); // returns {10,20,30,40}
```

## RemoveWhere({selector})
Removes the specific items from a list according to the specific conditions which declared by {selector} as a `Func()`.
#### When to use it?
When you want to remove some specific items from a list in your applications.

#### Example:
```csharp
var removeList = new List<string>() { "2", "3" };
removeList.RemoveWhere((r) => r == "2"); // "2" removed from removeList.
```
## RemoveWhereKey({criteria})
Removes the specific items from a dictionary. The condition declared by {criteria} as a `Func()`.
#### When to use it?
When you want to remove the specific items from a dictionary in your applications.

#### Example:
```csharp
var dictionary1 = new Dictionary<string, int>()
{
    {"mac", 1000},
    {"windows", 500}
};

dictionary.RemoveWhereKey((rr) => rr=="mac"); //"mac" item removed.
dictionary.RemoveWhereKey((rr) => rr=="windows"); //"windows" item removed.
dictionary.RemoveWhereKey((rr) => rr.Contains("a")); //All items which has "a" character in them removed.
```

## Replace({oldItem},{newItem},{caseSensitive})
Replaces the first specified item in `this` list with the specified new item.
#### When to use it?
When you want to replace the specified item in `this` list with the specified new item in your applications.
#### Example:
```csharp
IList<int?> alist = new List<int?>();
alist.Add(10);
alist.Add(20);
alist.Add(30);
alist.Add(40);
alist.Add(null);
alist.Replace(10, 60); // returns {60,20,30,40,null}
```

```csharp
IList<string> alist = new List<string>();
alist.Add("a");
alist.Add("b");
alist.Add("c");
alist.Add("A");
alist.Replace("A", "e",false); // returns {"e","b","c","A"}
alist.Replace("A", "e",true); // returns {"a","b","c","A"}
```

## ReplaceAll({oldItem},{newItem},{caseSensitive})
Replaces all specified items in `this` list with the specified new item.
#### When to use it?
When you want to replace the specified item in `this` list with the specified new item in your applications.
#### Example:

```csharp
IList<string> alist = new List<string>();
alist.Add("a");
alist.Add("b");
alist.Add("c");
alist.Add("A");
alist.ReplaceAll("A", "e",false); // returns {"e","b","c","e"}
alist.ReplaceAll("A", "e",true); // returns {"a","b","c","e"}
```

## Sum({selector})
Returns the sum of a timespan selector on `this` list.
#### When to use it?
When you want to return the sum of a `timespan` selector on `this` list in your applications.
This method adds up all values in an `IEnumerable`. It computes the sum total of all the numbers in an `array`, or `List`, of integers. 
This extension method in `LINQ` provides an excellent way to do this with minimal calling code.
#### Example:
```csharp
IList<int?> aa = new List<int?>();
aa.Add(10);
aa.Add(20);
aa.Add(30);
aa.Add(40);
aa.Add(null);
aa.Sum((er) => er); //returns 100
```

## Take({lowerBound},{upperBound})
Returns a subset of the items in a given collection in a range including the items at lower and upper bounds.
{lowerbound} should be smaller than {upperbound}. The default value of {upperbound} is {lowerbound}.

#### When to use it?
When you want to return a subset of the items in a given collection in a range including the items at lower and upper bounds in your applications.

#### Example:
```csharp
int?[] intstr2 = {10,20,30,40,50};
intstr2.Take(1,3); // returns {20,30,40}
intstr2.Take(0,1); // returns {10,20}
intstr2.Take(1,2); // returns {20,30}
intstr2.Take(0); // returns {}
intstr2.Take(1); // returns {10}
intstr2.Take(2); // returns {20}
intstr2.Take(2,1); // throws "lower bound should be smaller than upper bound." error.
```

## ToDictionary()
Converts a `KeyValuePair` `IEnumerable object` to a dictionary.
#### When to use it?
When you want to convert a `KeyValuePair` `IEnumerable object` to a dictionary in your applications.
#### Example:
```csharp
var list = new List<KeyValuePair<string, int>>();
list.Add(new KeyValuePair<string, int>("Cat", 1));
list.Add(new KeyValuePair<string, int>("Dog", 2));
list.Add(new KeyValuePair<string, int>("Rabbit", 4));
var Newlist= list6.ToDictionary(); // returns {[Cat,1],[Dog,2],[Rabbit,3]}
```            
```csharp
var list = new List<KeyValuePair<string, int>>();
var Newlist= list6.ToDictionary(); // returns {}
```            

## ToFormatString({format string},{separator},{lastSeparator})
Concatenates all members of `this` object as the {format string} and inserts {lastSeparator} between last two strings.  
If {lastSeparator} is not declared, it is equal to {separator}.
#### When to use it?
When you want to concatenate all members of `this` string into a `string` in your applications.

#### Example:
```csharp
string[] dtr = { "str1", "str2","str3","str4" };
dtr.ToFormatString("The item is {0}","_","-"); //returns "The item is str1_The item is str2_The item is str3-The item is str4"
```

## ToHashSet()
Returns a `HashSet` of type T (use for performance in place of ToList()).
`HashSet` is an optimized set collection. It helps eliminates duplicate strings or elements in an `array`. It is a set that hashes its contents.
#### When to use it?
When you want to return a `HashSet` of type T (use for performance in place of ToList()) in your applications.
#### Example:
```csharp
IEnumerable<string> items2 = new string[] { "abc","cde" };
var cd = items2.ToHashSet(); // returns a HashSet object
```

## ToLinesString()
Concatenates all elements in `this` list with Environment.NewLine.
#### When to use it?
When you want to Concatenate all elements in `this` list with Environment.NewLine in your applications.

#### Example:
```csharp
string[] dtr = { "str1", "str2","str3","str4","str1" };
dtr.ToLinesString(); //returns "str1\r\nstr2\r\nstr3\r\nstr4\r\nstr1"
```
```csharp
string[] dtr = {};
dtr.ToLinesString(); //returns ""
```
```csharp
IList<int?> aa = new List<int?>();
aa.Add(10);
aa.Add(20);
aa.Add(30);
aa.Add(40);
aa.Add(null);

var ff3 = aa.ToLinesString(); //returns "10\r\n20\r\n30\r\n40\r\n{NULL}"
```
## ToNameValueCollection()
Converts `this` to a `KeyValueCollection` and returns a `NameValueCollection` object.
> `NameValueCollection` represents a collection of associated `String` keys and `String` values that can be accessed either with the key or with the index.

#### When to use it?
When you want to convert `this` dictionary to a `KeyValueCollection` in your applications.
#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);

NameValueCollection = counts.ToNameValueCollection(); 
```
## ToString({separator},{lastSeparator})
Concatenates all members of `this` object and inserts {separator} among them and inserts {lastSeparator} between last two members.  
If {lastSeparator} is not declared, it is equal to {separator}.
#### When to use it?
When you want to concatenate all members into a string in your applications.

#### Example:
```csharp
string[] dtr = { "str1", "str2","str3" };
dtr.ToString(" "); // returns "str1 str2 str3"
dtr.ToString(" ","_"); // returns "str1 str2_str3"
dtr.ToString("I","B"); // returns "str1Istr2Bstr3"
dtr.ToString("I"); // returns "str1Istr2Istr3"
dtr.ToString("",""); // returns "strstr2str3"
```

## Trim()
Trims all elements in `this` list and excludes all `null` and "empty string" elements from the list.
#### When to use it?
When you want to trim all elements in `this` list in your applications.

#### Example:
```csharp
string[] dtr = { "  str1", "str2   "," str3","str4   ","   str1    " };
dtr.Trim(); // returns {"str1","str2","str3","str4","str1"}
```
## TryGet({key})
Gets the value with the specified key, or `null`.
This method optimizes Dictionary usage. It gets a value (at a key) from a `Dictionary`. And it eliminates unneeded lookups, making programs better.
#### When to use it?
When you want to get the value with the specified key, or `null` in your applications.

#### Example:
```csharp
var counts = new Dictionary<string, int>();
counts.Add("key0", 0);
counts.Add("key1", 1);
counts.Add("key2", 2);
counts.Add("key3", 3);
counts.Add("key4", 4);
counts.TryGet("key3"); // returns "key3"
```
## TryRemove({key},{caseSensitive})
Tries to the remove an item with the specified key from `this` dictionary.
#### When to use it?
When you want to try to the remove an item with the specified key from `this` dictionary in your applications.
#### Example:
```csharp
ConcurrentDictionary<string, int> _concurrent = new ConcurrentDictionary<string, int>();
_concurrent.TryAdd("0", 0);
_concurrent.TryAdd("10", 1);
_concurrent.TryAdd("20", 2);
_concurrent.TryAdd("30", 3);
_concurrent.TryAdd("40", 4);
_concurrent.TryAdd("50", 5);

_concurrent.TryRemove("10"); // returns {[50,5],[40,4],[30,3],[20,2],[10,1],[0,0]}
```
```csharp
ConcurrentDictionary<string, int> _concurrent = new ConcurrentDictionary<string, int>();
_concurrent.TryAdd("A", 0);
_concurrent.TryAdd("B", 1);
_concurrent.TryAdd("C", 2);
_concurrent.TryAdd("D", 3);
_concurrent.TryAdd("E", 4);
_concurrent.TryAdd("F", 5);

_concurrent.TryRemove("c",false); // returns {["F",5],["E",4],["D",3],["B",1],["A",0]}
_concurrent.TryRemove("c",true); // returns {["F",5],["E",4],["D",3],["C",2],["B",1],["A",0]} because caseSensitive is true, so "C" is not removed
```



## TryRemoveAt({index},{caseSensitive})
Tries to the remove an item with the specified key from `this` dictionary.
#### When to use it?
When you want to try to the remove an item with the specified key from `this` dictionary in your applications.
> `ConcurrentDictionary`: This type from System.Collections.Concurrent allows multiple threads to access a `Dictionary` instance. With it, you get a thread-safe, hash-based lookup table.
> The `ConcurrentDictionary` type resides in System.Collections.Concurrent. It was introduced in .NET 4.0.

#### Example:
```csharp
ConcurrentDictionary<string, int> _concurrent = new ConcurrentDictionary<string, int>();
_concurrent.TryAdd("0", 0);
_concurrent.TryAdd("10", 1);
_concurrent.TryAdd("20", 2);
_concurrent.TryAdd("30", 3);
_concurrent.TryAdd("40", 4);
_concurrent.TryAdd("50", 5);
```
```csharp
_concurrent.TryRemoveAt(0); // returns {[40,4],[30,3],[20,2],[10,1],[0,0]}
```
```csharp
_concurrent.TryRemoveAt(1); // returns {[50,5],[30,3],[20,2],[10,1],[0,0]}
```
```csharp
_concurrent.TryRemoveAt(5); // returns {[50,5],[40,4],[30,3],[20,2],[10,1]}
```
```csharp
_concurrent.TryRemoveAt(6); // returns {[50,5],[40,4],[30,3],[20,2],[10,1],[0,0]}
```
```csharp
_concurrent.TryRemoveAt(-1); // returns {[50,5],[40,4],[30,3],[20,2],[10,1],[0,0]}
```


## Union({otherLists},{caseSensitive})
Returns the union of `this` list with the specified other lists
> It removes duplicated items automatically and, result items are unique.
#### When to use it?
When you want to return the union of `this` list with the specified other lists in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] { "item1" };
IList<string> items2 = new string[] { "ITEM1" ,"item2","items3"};

items.Union(items2,false); // returns { "item1" ,"item2","items3". It is noticed that "item1" is duplicated in both lists and caseSensitive is false, so it is occurred once in result list.
items.Union(items2,true); // returns { "item1","ITEM1" ,"item2","items3". It is noticed that "item1" is duplicated in both lists and caseSensitive is true, so it is occurred twice in result list.
```

## WithMax({keySelector})
Selects the item with maximum of the specified value.
If this list is `empty`, `NULL` (or default of T) will be returned.
#### When to use it?
When you want to select the item with maximum of the specified value in your applications.

#### Example:
```csharp
int[] array1 = {3,2,1 };
array1.WithMax((tt) => tt >2); // returns 3
array1.WithMax((tt) => tt >1); // returns 2

int[] array2 = {4,1,2,3,1,0};
array2.WithMax((tt) => tt% 2 ==0); // returns 2

int[] array3 = {};
array3.WithMax((tt) => tt% 2 ==0); // returns 0

int[] array3 = {Null};
array3.WithMax((tt) => tt% 2 ==0); // returns Null
```
## WithMin({keySelector})
Selects the item with minimum of the specified value.
If this list is `empty`, `NULL` (or default of T) will be returned.
#### When to use it?
When you want to select the item with minimum of the specified value in your applications.

#### Example:
```csharp
int[] array1 = {3,2,1 };
array1.WithMin((tt) => tt >2); // returns 1
array1.WithMin((tt) => tt >1); // returns 1

int[] array2 = {4,1,2,3,1,0};
array2.WithMin((tt) => tt% 2 ==0); // returns 1

int[] array3 = {};
array3.WithMin((tt) => tt% 2 ==0); // returns 0

int[] array3 = {Null};
array3.WithMin((tt) => tt% 2 ==0); // returns Null
```

