
# Linq Extension Methods
> `Linq` extension methods are really useful in all frameworks for working with IEnumerable objects.
> `Olive` has various methods which you can use in your applications.


## ToString({separator},{lastSeparator})
concatenates all members of this object and inserts {separator} among them and inserts {lastSeparator} between last two members.  
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

## ToFormatString({format string},{separator},{lastSeparator})
concatenates all members of this object as the {format string} and inserts {lastSeparator} between last two strings.  
If {lastSeparator} is not declared, it is equal to {separator}.
#### When to use it?
When you want to concatenate all members of this string into a string in your applications.

#### Example:
```csharp
string[] dtr = { "str1", "str2","str3","str4" };
dtr.ToFormatString("The item is {0}","_","-"); //returns "The item is str1_The item is str2_The item is str3-The item is str4"
```

## LacksKey({keyString})
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

dictionary.LacksKey("windows"); // returns false
dictionary.LacksKey("window")' // returns true
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

## IndexOf({keyString},{Criteria Func})
Determines whether this value has {keyString} or not. If it has, this method returns its index. Otherwise, it returns -1.
Another override gets a Func<> which determines the criteria of index of items.
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

dtr.IndexOf((tr) => tr=="str"+"3"); //returns 2
dtr.IndexOf((tr) => tr=="str"+"1"); //returns 0
dtr.IndexOf((tr) => tr=="str"+"6"); //returns -1
```

## RemoveWhere({selector})
Removes the specific items from a list according to the specific conditions which declared by {selector} as a Func().
#### When to use it?
When you want to remove some specific items from a list in your applications.

#### Example:
```csharp
var removeList = new List<string>() { "2", "3" };
removeList.RemoveWhere((r) => r == "2"); // "2" removed from removeList.
```

## Add({dictionary})
Adds all items from a specified dictionary to this dictionary.
#### When to use it?
When you want to add all items from a specified dictionary to this dictionary in your applications.

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

## RemoveWhereKey({criteria})
Removes the specific items from a dictionary. The condition declared by {criteria} as a Func().
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

## Except({item},{criteria})
Gets all items of this list except those meeting a specified criteria.
{criteria} is Exclusion criteria.
#### When to use it?
When you want to get all items of this list except those meeting a specified criteria in your applications.

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

## Except({item},{criteria},{alsoDistinct})
Gets all items of this list except those meeting a specified criteria. 
{criteria} is Exclusion criteria.
if {alsoDistinct} is true, it returns distinct elements from a sequence. The default value is false.

#### When to use it?
When you want to get all items of this list except those meeting a specified criteria in your applications.

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
Gets all Non-NULL items of this list.

#### When to use it?
When you want to get all Non-NULL items of this list in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"msg","item1","item2",null};
items2= items.ExceptNull(); // null is removed. item2 has not null item.
```

## IsSingle()
Determines whether this value has one item or not. If this value has one item, it returns true. Otherwise, it returns false.

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


## Prepend({otherlist})
Adds the specified list to the beginning of this list.

#### When to use it?
When you want to add the specified list to the beginning of this list in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"ggg"};
IEnumerable<string> items2 = new string[] { "abc" };
IEnumerable<string> itemsResult = new string[] {};

itemsResult = items2.Prepend(items); // returns "ggg" and "abc"
```

## Concat({otherlist})
Concatenates all items of a list to this object.

#### When to use it?
When you want to concatenate an item to this object in your applications.

#### Example:
```csharp
IEnumerable<string> items = new string[] {"item1"};
IEnumerable<string> items2 = new string[] { "item2", null};
items2= items2.Concat(items); // returns {"item1","item2", null}
```

## AddRange({otherlist})
Adds all items of a list to this object.

#### When to use it?
When you want to add an item to this object in your applications.

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

## MinOrDefault({criteria})
Gets the minimum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
{criteria} is a Func() which determines the condition.
#### When to use it?
When you want to get the minimum value of a specified expression in this list in your applications.

#### Example:
```csharp
IList<string> dtr2 = new string[] { "str5", "str2", "str3", "str1" };
dtr2.MinOrDefault(fx => fx.ToUpper()); // returns "STR1"

IList<string> dtr2 = new string[] { "5", "4", "3", "2" };
dtr2.MinOrDefault(fx => fx= fx+10); // returns "210"
```

## MaxOrDefault({criteria})
Gets the maximum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
{criteria} is a Func() which determines the condition.
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
Gets the maximum value of the specified expression in this list. 
If no items exist in the list then null will be returned. 
{criteria} is a Func() which determines the condition.
#### When to use it?
When you want to get the maximum value of the specified expression in this list in your applications.

#### Example:
```csharp
int?[] intstr = { 1, 2, 3, 4, 5, 6 };
intstr.MaxOrNull(fx => fx= fx+10); // returns 16

```

## MinOrNull({criteria})
Gets the minimum value of the specified expression in this list. 
If no items exist in the list then null will be returned. 
{criteria} is a Func() which determines the condition.
#### When to use it?
When you want to get the minimum value of the specified expression in this list in your applications.

#### Example:
```csharp
int?[] intstr = { 1, 2, 3, 4, 5, 6 };
intstr.MinOrNull(fx => fx= fx+10); // returns 11

```

## IsSubsetOf({targetlist})
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

## IsEquivalentTo({targetlist})
Determines whether this list is equivalent to another specified list. Items in the list should be distinct for accurate result.


#### When to use it?
When you want to whether this list is equivalent to another specified list in your applications.

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

## Count({criteria})
Counts the number of items in this list matching the specified criteria.
{criteria} is a Func() which determines the condition.
#### When to use it?
When you want to count the number of items in this list matching the specified criteria in your applications.

#### Example:
```csharp
int?[] intstr1 = { 1,2,1,6 };
intstr.Count((xa) => xa==1); // returns 2

int?[] intstr = {};
intstr.Count((xa) => xa==1); // returns 0

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

## Lacks({item})
Works as opposite of Contains().

#### When to use it?
When you want to determines whether this list object does not contain the specified item in your applications.

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

## LacksAny({item})
Determines if this list lacks any item in the specified list.

#### When to use it?
When you want to determines if this list lacks any item in the specified list in your applications.

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

## LacksAll({item})
Determines if this list lacks all items in the specified list.
If all members of two lists are completely different, it returns true; otherwise, false.

#### When to use it?
When you want to determines if this list lacks any item in the specified list in your applications.

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

## Take({lowerBound},{upperBound})
Returns a subset of the items in a given collection in a range including the items at lower and upper bounds.
{lowerbound} should be smaller than {upperbound}.

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

## Distinct({selector})
Returns distinct elements from a sequence by using the default equality comparer to compare values.

#### When to use it?
When you want to return distinct elements from a sequence in your applications.

#### Example:
```csharp
int?[] intstr2 = { 10,20,30,40,50,10,10,20};
intstr2.Distinct(); // returns {10,20,30,40,50}
```

## ContainsAll({Items})
Determines of this list contains all items of another given list.

#### When to use it?
When you want to determine of this list contains all items of another given list in your applications.

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

## ContainsAny({Items})
Determines if this list contains any of the specified items.

#### When to use it?
When you want to determine if this list contains any of the specified items in your applications.

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

## None({criteria})
Determines if none of the items in this list meet a given criteria.
If the {criteria} is not declared, it determines if this is null or an empty list.
#### When to use it?
When you want to determine if none of the items in this list meet a given criteria in your applications.

#### Example:
```csharp
int[] array = { 1, 2, 3 };
array.Any(item => item % 2 == 0); // returns True. See if any elements are divisible by two.
array.Any(item => item > 3);// returns False. See if any elements are greater than three.
array.Any(item => item == 2); // returns True. See if any elements are 2.
array.Any(); // returns True. See if any elements are Null.
```

```csharp
int?[] array2 = { 1, 2, 3,null };
array2.Any(); // returns true. See if any elements are Null.
```

```csharp
int?[] array2 = { };
array2.Any();// returns false. See if any elements are Null.
```

## Intersects({otherList})
Determines if this list intersects with another specified list.
#### When to use it?
When you want to determine if this list intersects with another specified list in your applications.

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

## WithMax({keySelector})
Selects the item with maximum of the specified value.
If this list is empty, NULL (or default of T) will be returned.
#### When to use it?
When you want to Selects the item with maximum of the specified value in your applications.

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
If this list is empty, NULL (or default of T) will be returned.
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

## GetElementAfter({item})
Gets the element after a specified item in this list.
If the specified element does not exist in this list, an ArgumentException will be thrown.
If the specified element is the last in the list, NULL will be returned.
#### When to use it?
When you want to get the element after a specified item in this list in your applications.

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
```

## GetElementBefore({item})
Gets the element before a specified item in this list.
If the specified element does not exist in this list, an ArgumentException will be thrown.
If the specified element is the first in the list, NULL will be returned.
#### When to use it?
When you want to get the element after a specified item in this list in your applications.

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
Adds a item to a List with a specific format and add \r\n to the end of this item.
#### When to use it?
When you want to add a item to a List with a specific format in your applications.

#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("b");
list.AddLine("Hello"); // returns {"aa","b","Hello \r\n"}
```

## AddLine({text})
Adds the {test} and \r\n to the item and adds item to this List.
#### When to use it?
When you want to add the {test} and \r\n to the item and adds item to this List in your applications.

#### Example:
```csharp
var list = new List<String>();
list.Add("aa");
list.Add("b");
list.AddLine("Hello"); // returns {"aa","b","Hello \r\n"}
```

## Remove({itemsToRemove})
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


