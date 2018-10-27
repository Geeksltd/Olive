

# String Extension Methods
>`String`  extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.

# AllIndexesOf({pattern})
Gets all indexes of a specified string inside this text.
Finds {pattern} into this string and returns Inumerable value.
#### When to use it?
When you want to find a string into another string in your applications.

#### Example:
```csharp
"abcde".AllIndexesOf("b").ToList();
returns:
Count = 1
    [0]: 1
```
```csharp
"abcde".AllIndexesOf("b").ToList()[0]; //returns 1
```
```csharp
"abcbe".AllIndexesOf("b").ToList();
returns:
Count = 2
    [0]: 1
    [1]: 3
```
```csharp
"abcbe".AllIndexesOf("a").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"abcba".AllIndexesOf("a").ToList();
returns:
Count = 2
    [0]: 0
    [1]: 4
```
```csharp
"aaaaaa".AllIndexesOf("a").ToList();
returns:
Count = 6
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
    [4]: 4
    [5]: 5
```
```csharp
"aaaaaa".AllIndexesOf("aa").ToList();
returns:
Count = 5
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
    [4]: 4
 ```
 ```csharp
"aaaaaa".AllIndexesOf("aaa").ToList();
returns:
Count = 4
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
```
```csharp
"aaaaaa".AllIndexesOf("aaaa").ToList();
returns:
Count = 3
    [0]: 0
    [1]: 1
    [2]: 2
```
```csharp
"aaaaaa".AllIndicesOf("aaaaa").ToList();
returns:
Count = 2
    [0]: 0
    [1]: 1
```
```csharp
"aaaaaa".AllIndexesOf("aaaaaa").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"aaaaaa".AllIndexesOf("aaaaaaa").ToList(); //returns Count = 0
```

# TrimStart({searchString},{IscaseSensitive})
Removes the specified text from the start of this string instance.
Finds {searchString} into this string and removes it from left side of this string. Default value is `true`.
If {IscaseSensitive} is false, `CaseSensitive` is not important. 
#### When to use it?
When you want to find a string into another string and remove it in your applications.

#### Example:
```csharp
"abbaa".TrimStart("a").ToString(); //returns "bbaa"
"abbaa".TrimStart("A").ToString(); //returns "abbaa"
"abbaa".TrimStart("A",false).ToString(); //returns "bbaa"
"abbaa".TrimStart("AB",false).ToString(); //returns "baa"
"abbaa".TrimStart("AB").ToString(); //returns "abbaa"
"abbaa".TrimStart("abb").ToString(); //returns "aa"
```
TrimStart() has another override method which has a char[] parameter. You can give a list of characters and this method removes all of them.

```csharp
"abbaa".TrimStart('a','c').ToString(); //returns "bbaa"
"abcdef".TrimStart('a','c').ToString(); //returns "bcdef"
"abcdef".TrimStart('a','b').ToString(); //returns "cdef"
"abcdef".TrimStart('a','b','c').ToString(); //returns "def"
"abcdef".TrimStart('c','b','a').ToString(); //returns "def"

```

# TrimEnd({numberOfCharacters})
Trims the end of this instance of string with the specified number of characters.
#### When to use it?
When you want to remove specific number of characters from end of another string in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd(12); //returns ""
"abcdef".TrimEnd(0); //returns "abcdef"
"abcdef".TrimEnd(1); //returns "abcde"
"abcdef".TrimEnd(2); //returns "abcd"
"abcdef".TrimEnd(3); //returns "abc"

```

# TrimEnd({unnecessaryText},{IscaseSensitive})
Trims some unnecessary text from the end of this string, if it exists.
If {IscaseSensitive} is false, `CaseSensitive` is not important. 
#### When to use it?
When you want to find a string into end of another string and remove it in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd("cf"); //returns "abcdef"
"abcdef".TrimEnd("ef"); //returns "abcd"
"abcdef".TrimEnd("abcdef"); //returns ""
"abcdef".TrimEnd(""); //returns "abcdef"
"abcdef".TrimEnd("abcdeF",false); //returns ""
"abcdef".TrimEnd("abcdeF",true); //returns "abcdef"
"abcdef".TrimEnd("f",false); //returns "abcde"
"abcdef".TrimEnd("F",false); //returns "abcde"
"abcdef".TrimEnd("F",true); //returns "abcdef"


```

# TrimEnd({unnecessaryChar[]})
Trims the end of this instance of string with the specified number of characters.
Finds {searchString} into this string and removes it from left side of this string. Default value is `true`.
#### When to use it?
When you want to find some characters into another string and remove them in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd('e','c','f'); //returns "abcd"
"abcdef".TrimEnd('e','c',''); // throws error CS1011: Empty character literal
"abcdef".TrimEnd('g'); //returns "abcdef" because of 'g' is not occure in "abcdef"

```
# Right({length})
Returns the last few characters of the string with a length specified by the given parameter. If the string's length is less than the 
iven length the complete string is returned. If length is zero or less an empty string is returned.
{Length} is the number of characters to return.
#### When to use it?
When you want to get specific number of last characters of this string in your applications.

#### Example:
```csharp   
"abcdef".Right(0); //returns ""
"abcdef".Right(1); //returns "f"
"abcdef".Right(2); //returns "ef"
"abcdef".Right(3); //returns "def"
"abcdef".Right(6); //returns "abcdef"
"abcdef".Right(7); //returns "abcdef"
  ```    
  
# Left({length})
Returns the first few characters of the string with a length specified by the given parameter. If the string's length is less than the 
iven length the complete string is returned. If length is zero or less an empty string is returned.
{Length} is the number of characters to return.
#### When to use it?
When you want to get specific number of first characters of this string in your applications.

#### Example:
```csharp   
"abcdef".Left(0); //returns ""
"abcdef".Left(1); //returns "a"
"abcdef".Left(2); //returns "ab"
"abcdef".Left(3); //returns "abc"
"abcdef".Left(6); //returns "abcdef"
"abcdef".Left(7); //returns "abcdef"
  ```    

# FormatWith({listofArgs})
This method identifies a string literal as an `interpolated string`. 
An `interpolated string` is a string literal that might contain interpolated expressions.
When an interpolated string is resolved to a result string, items with interpolated expressions are replaced by the string representations of the expression results.
`String interpolation` provides a more readable and convenient syntax to create formatted strings than a string composite formatting feature. The following example uses both features to produce the same output.

#### When to use it?
When you want to insert some string in specific places in this string in your applications.

#### Example:
```csharp   
"There are {0} items in your invoice No.:{1} and bill No.:{2}".FormatWith("5","13570320","998845"); //returns "There are 5 items in your invoice No.:13570320 and bill No.:998845"
```

# GetLastChar()
Gets the last `Char` of a `string`.
#### When to use it?
When you want to Gets the last `Char` of a `string` in your applications.

#### Example:
```csharp   
"there".GetLastChar(); //returns "e"
```

# StartsWithAny({Args})
Gets whether this string item begins with any of the specified items{Args}.
#### When to use it?
When you want to know a `String` begins with other strings in your applications.

#### Example:
```csharp   
"there".StartsWithAny("a"); //returns false
"there".StartsWithAny("t"); //returns true
"there".StartsWithAny("th"); //returns true
"there".StartsWithAny("the"); //returns true
"there".StartsWithAny("ther"); //returns true
"there".StartsWithAny("there"); //returns true
"there".StartsWithAny("there "); //returns false
"there".StartsWithAny("ere"); //returns false
"there".StartsWithAny("er","th"); //returns true
"there".StartsWithAny("the","th"); //returns true
```

# EndsWithAny({Args})
Gets whether this string item begins with any of the specified items{Args}.
It returns `true` if this string begins with one of {args}.
#### When to use it?
When you want to know a `String` begins with other strings in your applications.

#### Example:
```csharp   
"there".EndsWithAny("a"); //returns false
"there".EndsWithAny("t"); //returns false
"there".EndsWithAny("er"); //returns true
"there".EndsWithAny("ere"); //returns true
"there".EndsWithAny("here"); //returns true
"there".EndsWithAny("there"); //returns true
"there".EndsWithAny("there "); //returns false
"there".EndsWithAny("er","th"); //returns true
"there".EndsWithAny("er","ere"); //returns true
```

# RemoveHtmlTags()
Removes all Html tags from this html string.
#### When to use it?
It is useful when you want to show context of a Html document.
#### Example:
```csharp   
"<Br><b><span>Hello World</span></b></br>".RemoveHtmlTags();// returns "Hello World"
```

# WithPrefix({prefixString})
Returns this text with the specified `prefix` if this has a value. 
If this text is `empty` or `null`, it will return `empty` string.
#### When to use it?
When you want to show context of a Html document.
#### Example:
```csharp   
"there".WithPrefix("pre");// returns "prethere"
"there".WithPrefix("");// returns "there"
"there".WithPrefix("When ");// returns "When there"
```

# WithSuffix({suffixString})
Returns this text with the specified `suffix` if this has a value. 
If this text is `empty` or `null`, it will return `empty` string.
#### When to use it?
When you want to show context of a Html document.
#### Example:
```csharp   
"there".WithSuffix("pre");// returns "therepre"
"there".WithSuffix("");// returns "there"
"there".WithSuffix("When ");// returns "there When"
```

# WithWrappers({leftString},{rightString})
Wraps this text between the left and right wrappers, only if this has a value.
#### When to use it?
When you want to wrap this text between the left and right wrappers.
#### Example:
```csharp   
"there".WithWrappers("When "," are");// returns "When there are"
"".WithWrappers("When "," are");// returns ""
"there".WithWrappers("","");// returns "there"
```
# Repeat({repeatTimes},{seperator})
Repeats this text by the number of times specified, seperated with the specified {seperator}.
#### When to use it?
When you want to create specific number of a string in your application.
#### Example:
```csharp   
"there".Repeat(2);//returns "therethere"
"there".Repeat(2,"");//returns "therethere"
"there".Repeat(2," ");//returns "there there "
"there".Repeat(2,"a");//returns "thereatherea"
"there".Repeat(2,"A");//returns "thereAthereA"
```

# TrimOrNull()
If this string object is `null`, it will return `null`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"Sample string".TrimOrNull(); //returns "Sample string"
"".TrimOrNull(); //returns ""
string? nullstring= null;
nullstring..TrimOrNull(); // returns null

```
# TrimOrEmpty()
If this string object is `null`, it will return `empty`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"Sample string".TrimOrNull(); //returns "Sample string"
"".TrimOrEmpty(); //returns ""
string? nullstring= null;
nullstring..TrimOrEmpty(); // returns `empty`
```

# IsNoneOf()
If this string object is `null`, it will return `empty`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"".IsNoneOf();// returns true
"a".IsNoneOf();// returns true
"ab".IsNoneOf();// returns true
"ab".IsNoneOf("a");// returns true
"ab".IsNoneOf("c");// returns true
"ab".IsNoneOf("ab");// returns false
"ab".IsNoneOf("abc");// returns true
"ab".IsNoneOf("");// returns true
```
# ToLowerOrEmpty()
Returns a copy of this text converted to lower case. If it is null it will return empty string.
#### When to use it?
When you want to change all characters of a string to lowercase in your application.
#### Example:
```csharp 
"Sample String".ToLowerOrEmpty();// returns  "sample string"
"".ToLowerOrEmpty() ;// returns ""
string? nullstring= null;
nullstring..ToLowerOrEmpty(); // returns ""

```
# ToUpperOrEmpty()
Returns a copy of this text converted to upper case. If it is null it will return empty string.
#### When to use it?
When you want to change all characters of a string to uppercase in your application.
#### Example:
```csharp 
"Sample String".ToUpperOrEmpty();// returns  "SAMPLE STRING"
"".ToUpperOrEmpty() ;// returns ""
string? nullstring= null;
nullstring..ToUpperOrEmpty(); // returns ""
```

# IsAnyOf({Args})
Determines this value is one of the {Args}.
#### When to use it?
When you want to check whether this value is one of the arrays in your application.
#### Example:
```csharp 
"a".IsAnyOf("g","f"); // returns false
"a".IsAnyOf("g","a"); //returns true
"".IsAnyOf("g","f"); //returns false
"".IsAnyOf("g","f",""); //returns true

var list = new List<string>();
list.Add("aa");
list.Add("b");
string Sstring = "s";
Sstring.IsAnyOf(list).ToString(); // returns "false"
```
# EnsureStart({StartString},{caseSensitive})
If this value starts with {StartString}, it returns this value, otherwise it adds {StartString} to this value and return combined string.
{caseSensitive} determines whether sensitivity is checked or not.
#### When to use it?
When you want to add a `string` to other if it is not in the start of other in your application.
#### Example:
```csharp 
"Sample String".EnsureStart("Sample"); // returns "SampleSample String"
"Sample String".EnsureStart("The "); // returns "The Sample String"
"Sample String".EnsureStart("The ",true); // returns "The Sample String"
"Sample String".EnsureStart("The ",false); // returns "The Sample String"
"Sample String".EnsureStart("the ",false); // returns "the Sample String"
"Sample String".EnsureStart("the ",true); // returns "the Sample String"
"Sample String".EnsureStart("Sample ",true); // returns "Sample String"
"Sample String".EnsureStart("Sample ",false); // returns "Sample Sample String"
"Sample String".EnsureStart("sample ",false); // returns "sample Sample String"
"Sample String".EnsureStart("sample ",true); // returns "Sample String"
```

# ContainsAll({Args},{caseSensitive})
If this string have one of the {Args} parameter, it returns `true`, otherwise it returns `false`.
{caseSensitive} determines whether sensitivity is checked or not. Default value is `false`.
#### When to use it?
When you want to control containing a string other strings in your application.
#### Example:
```csharp 
"Sample String".ContainsAll(""); // returns true
"Sample String".ContainsAll(" "); // returns true
"Sample String".ContainsAll("sng"); // returns false
"Sample String".ContainsAll("string"); // returns false
"Sample String".ContainsAll("String"); // returns true
```
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("b");
"bgaaff".ContainsAll(list.ToArray(),false).ToString();// returns "True"
"bgAAff".ContainsAll(list.ToArray(),true).ToString();// returns "False"
"acc".ContainsAll(list.ToArray(),false).ToString();// returns "False"
```
# IsEmpty()
Determines whether this instance of string is `null` or `empty`.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
string nullstring = null;
nullstring.IsEmpty();// returns true
"".IsEmpty();// returns true
"a".IsEmpty();// returns false
" ".IsEmpty();// returns false
```
# HasValue()
Determines whether this instance of string is `null` or `empty`.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
string nullstring = null;
nullstring.HasValue();// returns true
"".HasValue();// returns true
"a".HasValue();// returns false
" ".HasValue();// returns false
```
# ToHtmlLines()
Will replace all line breaks with a BR tag and return the result as a raw html.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
"Hello World\n".ToHtmlLines();// returns "Hello World<br/>"
"Hello World\n\r".ToHtmlLines();// returns "Hello World<br/>"
"Hello World\r".ToHtmlLines();// returns "Hello World"
"Hello World\n\r".ToHtmlLines();// returns "Hello World<br/>"
```
This method has another override that join all items with a <BR> tag and return the result as a raw html.
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("b");
list.ToHtmlLines();// returns  "aa<br/>b"
```
# Or({defaultValue})
Gets the same string if it is not null or empty. Otherwise it returns the specified default value.
#### When to use it?
When you want to fill a string with {defaultvalue} if it is `Empty` or `Null` in your application.
#### Example:
```csharp 
"".Or("a"); //returns "a"
string nullstring = null;
nullstring.Or("Sample);// returns "Sample"
```
# Unless({unwantedText})
Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
#### When to use it?
When you want to the same string unless it is the same as the specified text in your application.
#### Example:
```csharp 
"".Unless(false); //returns ""
"".Unless(true); //returns ""
"".Unless("Sample"); //returns ""
"Example".Unless("Sample"); //returns "Example"
"Example".Unless(""); //returns "Example"
"Example".Unless("Example"); //returns ""
"Example".Unless("example"); //returns "Example"
```
# Summarize({maximumlenght},{enforcemaxlenght})
Summarizes the specified source.
If {enforcemaxlenght} is smaller than 3, you get an error. It should be greater than 3.
#### When to use it?
When you want to show a few number of chasracters of a string and show other characters like ... in your application.
#### Example:
```csharp 
"Example".Summarize(1,true); //returns Exception thrown
"Example".Summarize(7,true); //returns "Example"
"Example".Summarize(8,true); //returns "Example"
"Example".Summarize(6,true); //returns "Exa..."
"Example".Summarize(5,true); //returns "Ex..."
"Example".Summarize(4,true); //returns "E..."
"Example".Summarize(3,true); //returns "..."
"Example".Summarize(2,true); //returns Exception thrown

"Example".Summarize(0); //returns "..."
"Example".Summarize(1); //returns "E..."
"Example".Summarize(2); //returns "Ex..."
"Example".Summarize(3); //returns "Exa..."
"Example".Summarize(4); //returns "Exam..."
"Example".Summarize(5); //returns "Examp..."
"Example".Summarize(6); //returns "Exampl..."
"Example".Summarize(7); //returns "Example"
"Example".Summarize(8); //returns "Example"

```

# StartsWith({otherstring},{StringComparison})
 Gets whether this string item begins with any of the specified items.
> `StringComparison` enum influences string comparisons. It contains several constants. 
Many methods use a parameter of type StringComparison. It specifies the internal behavior of methods.
`StringComparison` values are used to tell string methods how to compare string data:
- CurrentCulture = 0
- CurrentCultureIgnoreCase = 1
- InvariantCulture = 2
- InvariantCultureIgnoreCase = 3
- Ordinal = 4
- OrdinalIgnoreCase = 5
`CurrentCulture` enumerated constant indicates that you want the called function to use a search based on the current machine's culture. The effect varies based on the machine's setup.
`Invariant` means that the culture itself is invariant and will remain the same without varying.
`Ordinal` means number-based. It indicates that the characters in the string should be treated by their numeric value. The capital letter "A" would always represent the number 65 because of the ASCII layout.
#### When to use it?
When you want to get whether this string item begins with any of the specified items in your application.
#### Example:
```csharp 
"Example".StartsWith("An "); //returns false
"Example".StartsWith("E"); //returns true
"Example".StartsWith("e"); //returns false
"Example".StartsWith("e",false); //returns true
"Example".StartsWith("e",true); //returns false
"Example".StartsWith("e",StringComparison.OrdinalIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.CurrentCulture); //returns false
"Example".StartsWith("e",StringComparison.CurrentCultureIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.InvariantCulture); //returns false
"Example".StartsWith("e",StringComparison.InvariantCultureIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.OrdinalIgnoreCase); //returns true
```
# Contains({Args},{caseSensitive})
If this string have one of the {Args} parameter, it returns `true`, otherwise it returns `false`.
{caseSensitive} determines whether sensitivity is checked or not. Default value is `false`.
#### When to use it?
When you want to control containing a string other strings in your application.
#### Example:
```csharp 
"Example".Contains("m"); //returns true
"Example".Contains("M"); //returns false
"Example".Contains("m",false); //returns true
"Example".Contains("M",false); //returns true
```
# Remove({firstSubstringsToRemove},{otherSubstringsToRemove})
Removes the specified substrings from this string object.
#### When to use it?
When you want to Remove some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".Remove("Sample "); //returns "Example"
"Sample Example".Remove("a"); //returns "Smple Exmple"
"Sample Example".Remove("a","b"); //returns "Smple Exmple"
"Sample Example".Remove("a","x"); //returns "Smple Emple"
"Sample Example".Remove("a","x","E"); //returns "Smple mple"
```

# KeepReplacing({original},{substitute})
Replaces all occurances of a specified phrase to a substitude, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
#### When to use it?
When you want to replace some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".KeepReplacing("xa","pe");// returns "Sample Epemple"
"Sample Example".KeepReplacing("a","p");// returns "Spmple Expmple"
"Sample Example".KeepReplacing(" ","");// returns "SampleExample"
```


# String Extension Methods
>`String`  extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.

# AllIndexesOf({pattern})
Gets all indexes of a specified string inside this text.
Finds {pattern} into this string and returns Inumerable value.
#### When to use it?
When you want to find a string into another string in your applications.

#### Example:
```csharp
"abcde".AllIndexesOf("b").ToList();
returns:
Count = 1
    [0]: 1
```
```csharp
"abcde".AllIndexesOf("b").ToList()[0]; //returns 1
```
```csharp
"abcbe".AllIndexesOf("b").ToList();
returns:
Count = 2
    [0]: 1
    [1]: 3
```
```csharp
"abcbe".AllIndexesOf("a").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"abcba".AllIndexesOf("a").ToList();
returns:
Count = 2
    [0]: 0
    [1]: 4
```
```csharp
"aaaaaa".AllIndexesOf("a").ToList();
returns:
Count = 6
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
    [4]: 4
    [5]: 5
```
```csharp
"aaaaaa".AllIndexesOf("aa").ToList();
returns:
Count = 5
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
    [4]: 4
 ```
 ```csharp
"aaaaaa".AllIndexesOf("aaa").ToList();
returns:
Count = 4
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
```
```csharp
"aaaaaa".AllIndexesOf("aaaa").ToList();
returns:
Count = 3
    [0]: 0
    [1]: 1
    [2]: 2
```
```csharp
"aaaaaa".AllIndicesOf("aaaaa").ToList();
returns:
Count = 2
    [0]: 0
    [1]: 1
```
```csharp
"aaaaaa".AllIndexesOf("aaaaaa").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"aaaaaa".AllIndexesOf("aaaaaaa").ToList(); //returns Count = 0
```

# TrimStart({searchString},{IscaseSensitive})
Removes the specified text from the start of this string instance.
Finds {searchString} into this string and removes it from left side of this string. Default value is `true`.
If {IscaseSensitive} is false, `CaseSensitive` is not important. 
#### When to use it?
When you want to find a string into another string and remove it in your applications.

#### Example:
```csharp
"abbaa".TrimStart("a").ToString(); //returns "bbaa"
"abbaa".TrimStart("A").ToString(); //returns "abbaa"
"abbaa".TrimStart("A",false).ToString(); //returns "bbaa"
"abbaa".TrimStart("AB",false).ToString(); //returns "baa"
"abbaa".TrimStart("AB").ToString(); //returns "abbaa"
"abbaa".TrimStart("abb").ToString(); //returns "aa"
```
TrimStart() has another override method which has a char[] parameter. You can give a list of characters and this method removes all of them.

```csharp
"abbaa".TrimStart('a','c').ToString(); //returns "bbaa"
"abcdef".TrimStart('a','c').ToString(); //returns "bcdef"
"abcdef".TrimStart('a','b').ToString(); //returns "cdef"
"abcdef".TrimStart('a','b','c').ToString(); //returns "def"
"abcdef".TrimStart('c','b','a').ToString(); //returns "def"

```

# TrimEnd({numberOfCharacters})
Trims the end of this instance of string with the specified number of characters.
#### When to use it?
When you want to remove specific number of characters from end of another string in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd(12); //returns ""
"abcdef".TrimEnd(0); //returns "abcdef"
"abcdef".TrimEnd(1); //returns "abcde"
"abcdef".TrimEnd(2); //returns "abcd"
"abcdef".TrimEnd(3); //returns "abc"

```

# TrimEnd({unnecessaryText},{IscaseSensitive})
Trims some unnecessary text from the end of this string, if it exists.
If {IscaseSensitive} is false, `CaseSensitive` is not important. 
#### When to use it?
When you want to find a string into end of another string and remove it in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd("cf"); //returns "abcdef"
"abcdef".TrimEnd("ef"); //returns "abcd"
"abcdef".TrimEnd("abcdef"); //returns ""
"abcdef".TrimEnd(""); //returns "abcdef"
"abcdef".TrimEnd("abcdeF",false); //returns ""
"abcdef".TrimEnd("abcdeF",true); //returns "abcdef"
"abcdef".TrimEnd("f",false); //returns "abcde"
"abcdef".TrimEnd("F",false); //returns "abcde"
"abcdef".TrimEnd("F",true); //returns "abcdef"


```

# TrimEnd({unnecessaryChar[]})
Trims the end of this instance of string with the specified number of characters.
Finds {searchString} into this string and removes it from left side of this string. Default value is `true`.
#### When to use it?
When you want to find some characters into another string and remove them in your applications.

#### Example:
```csharp   
"abcdef".TrimEnd('e','c','f'); //returns "abcd"
"abcdef".TrimEnd('e','c',''); // throws error CS1011: Empty character literal
"abcdef".TrimEnd('g'); //returns "abcdef" because of 'g' is not occure in "abcdef"

```
# Right({length})
Returns the last few characters of the string with a length specified by the given parameter. If the string's length is less than the 
iven length the complete string is returned. If length is zero or less an empty string is returned.
{Length} is the number of characters to return.
#### When to use it?
When you want to get specific number of last characters of this string in your applications.

#### Example:
```csharp   
"abcdef".Right(0); //returns ""
"abcdef".Right(1); //returns "f"
"abcdef".Right(2); //returns "ef"
"abcdef".Right(3); //returns "def"
"abcdef".Right(6); //returns "abcdef"
"abcdef".Right(7); //returns "abcdef"
  ```    
  
# Left({length})
Returns the first few characters of the string with a length specified by the given parameter. If the string's length is less than the 
iven length the complete string is returned. If length is zero or less an empty string is returned.
{Length} is the number of characters to return.
#### When to use it?
When you want to get specific number of first characters of this string in your applications.

#### Example:
```csharp   
"abcdef".Left(0); //returns ""
"abcdef".Left(1); //returns "a"
"abcdef".Left(2); //returns "ab"
"abcdef".Left(3); //returns "abc"
"abcdef".Left(6); //returns "abcdef"
"abcdef".Left(7); //returns "abcdef"
  ```    

# FormatWith({listofArgs})
This method identifies a string literal as an `interpolated string`. 
An `interpolated string` is a string literal that might contain interpolated expressions.
When an interpolated string is resolved to a result string, items with interpolated expressions are replaced by the string representations of the expression results.
`String interpolation` provides a more readable and convenient syntax to create formatted strings than a string composite formatting feature. The following example uses both features to produce the same output.

#### When to use it?
When you want to insert some string in specific places in this string in your applications.

#### Example:
```csharp   
"There are {0} items in your invoice No.:{1} and bill No.:{2}".FormatWith("5","13570320","998845"); //returns "There are 5 items in your invoice No.:13570320 and bill No.:998845"
```

# GetLastChar()
Gets the last `Char` of a `string`.
#### When to use it?
When you want to Gets the last `Char` of a `string` in your applications.

#### Example:
```csharp   
"there".GetLastChar(); //returns "e"
```

# StartsWithAny({Args})
Gets whether this string item begins with any of the specified items{Args}.
#### When to use it?
When you want to know a `String` begins with other strings in your applications.

#### Example:
```csharp   
"there".StartsWithAny("a"); //returns false
"there".StartsWithAny("t"); //returns true
"there".StartsWithAny("th"); //returns true
"there".StartsWithAny("the"); //returns true
"there".StartsWithAny("ther"); //returns true
"there".StartsWithAny("there"); //returns true
"there".StartsWithAny("there "); //returns false
"there".StartsWithAny("ere"); //returns false
"there".StartsWithAny("er","th"); //returns true
"there".StartsWithAny("the","th"); //returns true
```

# EndsWithAny({Args})
Gets whether this string item begins with any of the specified items{Args}.
It returns `true` if this string begins with one of {args}.
#### When to use it?
When you want to know a `String` begins with other strings in your applications.

#### Example:
```csharp   
"there".EndsWithAny("a"); //returns false
"there".EndsWithAny("t"); //returns false
"there".EndsWithAny("er"); //returns true
"there".EndsWithAny("ere"); //returns true
"there".EndsWithAny("here"); //returns true
"there".EndsWithAny("there"); //returns true
"there".EndsWithAny("there "); //returns false
"there".EndsWithAny("er","th"); //returns true
"there".EndsWithAny("er","ere"); //returns true
```

# RemoveHtmlTags()
Removes all Html tags from this html string.
#### When to use it?
It is useful when you want to show context of a Html document.
#### Example:
```csharp   
"<Br><b><span>Hello World</span></b></br>".RemoveHtmlTags();// returns "Hello World"
```

# WithPrefix({prefixString})
Returns this text with the specified `prefix` if this has a value. 
If this text is `empty` or `null`, it will return `empty` string.
#### When to use it?
When you want to show context of a Html document.
#### Example:
```csharp   
"there".WithPrefix("pre");// returns "prethere"
"there".WithPrefix("");// returns "there"
"there".WithPrefix("When ");// returns "When there"
```

# WithSuffix({suffixString})
Returns this text with the specified `suffix` if this has a value. 
If this text is `empty` or `null`, it will return `empty` string.
#### When to use it?
When you want to show context of a Html document.
#### Example:
```csharp   
"there".WithSuffix("pre");// returns "therepre"
"there".WithSuffix("");// returns "there"
"there".WithSuffix("When ");// returns "there When"
```

# WithWrappers({leftString},{rightString})
Wraps this text between the left and right wrappers, only if this has a value.
#### When to use it?
When you want to wrap this text between the left and right wrappers.
#### Example:
```csharp   
"there".WithWrappers("When "," are");// returns "When there are"
"".WithWrappers("When "," are");// returns ""
"there".WithWrappers("","");// returns "there"
```
# Repeat({repeatTimes},{seperator})
Repeats this text by the number of times specified, seperated with the specified {seperator}.
#### When to use it?
When you want to create specific number of a string in your application.
#### Example:
```csharp   
"there".Repeat(2);//returns "therethere"
"there".Repeat(2,"");//returns "therethere"
"there".Repeat(2," ");//returns "there there "
"there".Repeat(2,"a");//returns "thereatherea"
"there".Repeat(2,"A");//returns "thereAthereA"
```

# TrimOrNull()
If this string object is `null`, it will return `null`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"Sample string".TrimOrNull(); //returns "Sample string"
"".TrimOrNull(); //returns ""
string? nullstring= null;
nullstring..TrimOrNull(); // returns null

```
# TrimOrEmpty()
If this string object is `null`, it will return `empty`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"Sample string".TrimOrNull(); //returns "Sample string"
"".TrimOrEmpty(); //returns ""
string? nullstring= null;
nullstring..TrimOrEmpty(); // returns `empty`
```

# IsNoneOf()
If this string object is `null`, it will return `empty`. Otherwise it will trim the text and return it.
#### When to use it?
When you want to remove space from both sides of a string in your application.
#### Example:
```csharp   
"".IsNoneOf();// returns true
"a".IsNoneOf();// returns true
"ab".IsNoneOf();// returns true
"ab".IsNoneOf("a");// returns true
"ab".IsNoneOf("c");// returns true
"ab".IsNoneOf("ab");// returns false
"ab".IsNoneOf("abc");// returns true
"ab".IsNoneOf("");// returns true
```
# ToLowerOrEmpty()
Returns a copy of this text converted to lower case. If it is null it will return empty string.
#### When to use it?
When you want to change all characters of a string to lowercase in your application.
#### Example:
```csharp 
"Sample String".ToLowerOrEmpty();// returns  "sample string"
"".ToLowerOrEmpty() ;// returns ""
string? nullstring= null;
nullstring..ToLowerOrEmpty(); // returns ""

```
# ToUpperOrEmpty()
Returns a copy of this text converted to upper case. If it is null it will return empty string.
#### When to use it?
When you want to change all characters of a string to uppercase in your application.
#### Example:
```csharp 
"Sample String".ToUpperOrEmpty();// returns  "SAMPLE STRING"
"".ToUpperOrEmpty() ;// returns ""
string? nullstring= null;
nullstring..ToUpperOrEmpty(); // returns ""
```
# IsAnyOf({Args})
Determines this value is one of the {Args}.
#### When to use it?
When you want to check whether this value is one of the arrays in your application.
#### Example:
```csharp 
"a".IsAnyOf("g","f"); // returns false
"a".IsAnyOf("g","a"); //returns true
"".IsAnyOf("g","f"); //returns false
"".IsAnyOf("g","f",""); //returns true

var list = new List<string>();
list.Add("aa");
list.Add("b");
string Sstring = "s";
Sstring.IsAnyOf(list).ToString(); // returns "false"
```
# EnsureStart({StartString},{caseSensitive})
If this value starts with {StartString}, it returns this value, otherwise it adds {StartString} to this value and return combined string.
{caseSensitive} determines whether sensitivity is checked or not.
#### When to use it?
When you want to add a `string` to other if it is not in the start of other in your application.
#### Example:
```csharp 
"Sample String".EnsureStart("Sample"); // returns "SampleSample String"
"Sample String".EnsureStart("The "); // returns "The Sample String"
"Sample String".EnsureStart("The ",true); // returns "The Sample String"
"Sample String".EnsureStart("The ",false); // returns "The Sample String"
"Sample String".EnsureStart("the ",false); // returns "the Sample String"
"Sample String".EnsureStart("the ",true); // returns "the Sample String"
"Sample String".EnsureStart("Sample ",true); // returns "Sample String"
"Sample String".EnsureStart("Sample ",false); // returns "Sample Sample String"
"Sample String".EnsureStart("sample ",false); // returns "sample Sample String"
"Sample String".EnsureStart("sample ",true); // returns "Sample String"
```

# ContainsAll({Args},{caseSensitive})
If this string have one of the {Args} parameter, it returns `true`, otherwise it returns `false`.
{caseSensitive} determines whether sensitivity is checked or not. Default value is `false`.
#### When to use it?
When you want to control containing a string other strings in your application.
#### Example:
```csharp 
"Sample String".ContainsAll(""); // returns true
"Sample String".ContainsAll(" "); // returns true
"Sample String".ContainsAll("sng"); // returns false
"Sample String".ContainsAll("string"); // returns false
"Sample String".ContainsAll("String"); // returns true
```
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("b");
"bgaaff".ContainsAll(list.ToArray(),false).ToString();// returns "True"
"bgAAff".ContainsAll(list.ToArray(),true).ToString();// returns "False"
"acc".ContainsAll(list.ToArray(),false).ToString();// returns "False"
```
# IsEmpty()
Determines whether this instance of string is `null` or `empty`.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
string nullstring = null;
nullstring.IsEmpty();// returns true
"".IsEmpty();// returns true
"a".IsEmpty();// returns false
" ".IsEmpty();// returns false
```
# HasValue()
Determines whether this instance of string is `null` or `empty`.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
string nullstring = null;
nullstring.HasValue();// returns true
"".HasValue();// returns true
"a".HasValue();// returns false
" ".HasValue();// returns false
```
# ToHtmlLines()
Will replace all line breaks with a BR tag and return the result as a raw html.
#### When to use it?
When you want to control a string is `Empty` or `Null` in your application.
#### Example:
```csharp 
"Hello World\n".ToHtmlLines();// returns "Hello World<br/>"
"Hello World\n\r".ToHtmlLines();// returns "Hello World<br/>"
"Hello World\r".ToHtmlLines();// returns "Hello World"
"Hello World\n\r".ToHtmlLines();// returns "Hello World<br/>"
```
This method has another override that join all items with a <BR> tag and return the result as a raw html.
```csharp
var list = new List<string>();
list.Add("aa");
list.Add("b");
list.ToHtmlLines();// returns  "aa<br/>b"
```
# Or({defaultValue})
Gets the same string if it is not null or empty. Otherwise it returns the specified default value.
#### When to use it?
When you want to fill a string with {defaultvalue} if it is `Empty` or `Null` in your application.
#### Example:
```csharp 
"".Or("a"); //returns "a"
string nullstring = null;
nullstring.Or("Sample);// returns "Sample"
```
# Unless({unwantedText})
Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
#### When to use it?
When you want to the same string unless it is the same as the specified text in your application.
#### Example:
```csharp 
"".Unless(false); //returns ""
"".Unless(true); //returns ""
"".Unless("Sample"); //returns ""
"Example".Unless("Sample"); //returns "Example"
"Example".Unless(""); //returns "Example"
"Example".Unless("Example"); //returns ""
"Example".Unless("example"); //returns "Example"
```
# Summarize({maximumlenght},{enforcemaxlenght})
Summarizes the specified source.
If {enforcemaxlenght} is smaller than 3, you get an error. It should be greater than 3.
#### When to use it?
When you want to show a few number of chasracters of a string and show other characters like ... in your application.
#### Example:
```csharp 
"Example".Summarize(1,true); //returns Exception thrown
"Example".Summarize(7,true); //returns "Example"
"Example".Summarize(8,true); //returns "Example"
"Example".Summarize(6,true); //returns "Exa..."
"Example".Summarize(5,true); //returns "Ex..."
"Example".Summarize(4,true); //returns "E..."
"Example".Summarize(3,true); //returns "..."
"Example".Summarize(2,true); //returns Exception thrown

"Example".Summarize(0); //returns "..."
"Example".Summarize(1); //returns "E..."
"Example".Summarize(2); //returns "Ex..."
"Example".Summarize(3); //returns "Exa..."
"Example".Summarize(4); //returns "Exam..."
"Example".Summarize(5); //returns "Examp..."
"Example".Summarize(6); //returns "Exampl..."
"Example".Summarize(7); //returns "Example"
"Example".Summarize(8); //returns "Example"

```

# StartsWith({otherstring},{StringComparison})
 Gets whether this string item begins with any of the specified items.
> `StringComparison` enum influences string comparisons. It contains several constants. 
Many methods use a parameter of type StringComparison. It specifies the internal behavior of methods.
`StringComparison` values are used to tell string methods how to compare string data:
- CurrentCulture = 0
- CurrentCultureIgnoreCase = 1
- InvariantCulture = 2
- InvariantCultureIgnoreCase = 3
- Ordinal = 4
- OrdinalIgnoreCase = 5
`CurrentCulture` enumerated constant indicates that you want the called function to use a search based on the current machine's culture. The effect varies based on the machine's setup.
`Invariant` means that the culture itself is invariant and will remain the same without varying.
`Ordinal` means number-based. It indicates that the characters in the string should be treated by their numeric value. The capital letter "A" would always represent the number 65 because of the ASCII layout.
#### When to use it?
When you want to get whether this string item begins with any of the specified items in your application.
#### Example:
```csharp 
"Example".StartsWith("An "); //returns false
"Example".StartsWith("E"); //returns true
"Example".StartsWith("e"); //returns false
"Example".StartsWith("e",false); //returns true
"Example".StartsWith("e",true); //returns false
"Example".StartsWith("e",StringComparison.OrdinalIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.CurrentCulture); //returns false
"Example".StartsWith("e",StringComparison.CurrentCultureIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.InvariantCulture); //returns false
"Example".StartsWith("e",StringComparison.InvariantCultureIgnoreCase); //returns true
"Example".StartsWith("e",StringComparison.OrdinalIgnoreCase); //returns true
```
# Contains({Args},{caseSensitive})
If this string have one of the {Args} parameter, it returns `true`, otherwise it returns `false`.
{caseSensitive} determines whether sensitivity is checked or not. Default value is `false`.
#### When to use it?
When you want to control containing a string other strings in your application.
#### Example:
```csharp 
"Example".Contains("m"); //returns true
"Example".Contains("M"); //returns false
"Example".Contains("m",false); //returns true
"Example".Contains("M",false); //returns true
```
# Remove({firstSubstringsToRemove},{otherSubstringsToRemove})
Removes the specified substrings from this string object.
#### When to use it?
When you want to Remove some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".Remove("Sample "); //returns "Example"
"Sample Example".Remove("a"); //returns "Smple Exmple"
"Sample Example".Remove("a","b"); //returns "Smple Exmple"
"Sample Example".Remove("a","x"); //returns "Smple Emple"
"Sample Example".Remove("a","x","E"); //returns "Smple mple"
```

# KeepReplacing({original},{substitute})
Replaces all occurances of a specified phrase to a substitude, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
#### When to use it?
When you want to replace some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".KeepReplacing("xa","pe");// returns "Sample Epemple"
"Sample Example".KeepReplacing("a","p");// returns "Spmple Expmple"
"Sample Example".KeepReplacing(" ","");// returns "SampleExample"
```

# OnlyWhen({condition})
Gets this same string when a specified condition is True, otherwise it returns empty string.
#### When to use it?
When you want to get this same string when a specified condition is True, otherwise it returns empty string in your application codes.
#### Example:
```csharp 
"Sample Example".OnlyWhen(true);// returns "Sample Example"
"Sample Example".OnlyWhen(false);// returns ""
```

# ToLines()
Gets the lines of this string.
#### When to use it?
When you want to split an string which contains \n or \r into an Array in your application codes.
#### Example:
```csharp 
"First Line\n\r Second Line\nThird Line".ToLines();
returns:
    [0]: "First Line"
    [1]: " Second Line"
    [2]: "Third Line"
```
# IsUpper()
Indicates whether this character is categorized as an uppercase letter.
#### When to use it?
When you want to indicate whether this character is categorized as an uppercase letter in your application codes.
#### Example:
```csharp 
'a'.IsUpper();// returns false
'A'.IsUpper();// returns true
''.IsUpper();// throws error: Empty character literal

```

# IsLower()
Indicates whether this character is categorized as an lowercase letter.
#### When to use it?
When you want to indicate whether this character is categorized as an lowercase letter in your application codes.
#### Example:
```csharp 
'a'.IsLower();// returns true
'A'.IsLower();// returns false
''.IsLower();// throws error: Empty character literal
```
# IsLetter()
Indicates whether this character is categorized as a letter.
#### When to use it?
When you want to indicate whether this character is categorized as a letter in your application codes.
#### Example:
```csharp 
'a'.IsLetter();// returns true
'A'.IsLetter();// returns true
'1'.IsLetter();// returns false
''.IsLetter();// throws error: Empty character literal
```

# IsDigit()
Indicates whether this character is categorized as a digit.
#### When to use it?
When you want to indicate whether this character is categorized as a digit in your application codes.
#### Example:
```csharp 
'a'.IsDigit();// returns false
'1'.IsDigit();// returns true
' '.IsDigit();// returns false
'!'.IsDigit();// returns false
''.IsDigit();// throws error: Empty character literal
```

# IsWhiteSpace()
Indicates whether this character is categorized as White Space (space, new line, etc).
#### When to use it?
When you want to indicate whether this character is categorized as White Space (space, new line, etc).
#### Example:
```csharp 
'A'.IsWhiteSpace();// returns false
'1'.IsWhiteSpace();// returns false
'\n'.IsWhiteSpace();// returns true
'\r'.IsWhiteSpace();// returns true
```

# IsLetterOrDigit()
Indicates whether this character is categorized as `Letter` or `Digit`.
#### When to use it?
When you want to indicate whether this character is categorized as as `Letter` or `Digit`.
#### Example:
```csharp 
'\r'.IsLetterOrDigit();// returns false
'\n'.IsLetterOrDigit();// returns false
'1'.IsLetterOrDigit();// returns true
'A'.IsLetterOrDigit();// returns true
'!'.IsLetterOrDigit();// returns false
```

# ToStringOrEmpty()
If this expression is null, returns an empty string. Otherwise, it returns the ToString() of this instance.
#### When to use it?
When you want to indicate whether this character is categorized as as `Letter` or `Digit`.
#### Example:
```csharp 
"Sample Example".ToStringOrEmpty();// returns "Sample Example"
"".ToStringOrEmpty();// returns ""
string nullstring = null;
nullstring.ToStringOrEmpty();// returns ""
```
# Lacks({phrase},{caseSensitive})
Determines whether this string object does not contain the specified phrase.
#### When to use it?
When you want to determines whether this string object does not contain the specified phrase.
#### Example:
```csharp 
"Sample Example".Lacks("Example",true);// returns false
"Sample Example".Lacks("aa",true);// returns true
"Sample Example".Lacks("aa");// returns true
```        
# LacksAll({phrasesArgs},{caseSensitive})
Determines whether this string object does not contain any of the specified phrases.
#### When to use it?
When you want to determines whether this string object does not contain any of the specified phrases.
#### Example:
```csharp 
"Sample Example".LacksAll("Example",true);// returns false
"Sample Example".LacksAll("aa",true);// returns true
"Sample Example".LacksAll("aa");// returns true
```
It has another override method that gives {caseSensitive} at first and after that it gives `String Arrays`.
```csharp
"Sample Example".LacksAll(false,"aa","bb","sample");// returns false
"Sample Example".LacksAll(true,"aa","bb","sample");// returns true
```   

# ToLiteralFromPascalCase()
Returns natural English literal text for a specified pascal case string value.
#### When to use it?
When you want to return natural English literal text for a specified pascal case string value in your application codes.
#### Example:
```csharp 
"ThisIsSomething".ToLiteralFromPascalCase();// returns "This is something"
```
# ToUpper()
It gives IEnumerable items and change all to uppercase items.
#### When to use it?
When you want to uppercase all items of an IEnumerable object in your application codes.
#### Example:
```csharp 
IEnumerable<string> items = new string[] {"msg","item1","item2" };
IEnumerable<string> items2 ;
items2 = items.ToUpper();
foreach (var item in items2)
{
...
}
```
# ToLower()
It gives IEnumerable items and change all to lowercase items.
#### When to use it?
When you want to lowercase all items of an IEnumerable object in your application codes.
#### Example:
```csharp 
IEnumerable<string> items = new string[] {"msg","item1","item2" };
IEnumerable<string> items2 ;
items2 = items.ToLower();
foreach (var item in items2) // returns {"MSG","ITEM1","ITEM2" }
{
... 
}
```
