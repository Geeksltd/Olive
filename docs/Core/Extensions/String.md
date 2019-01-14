# String Extension Methods
>`String`  extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.

# AllIndicesOf({pattern})
Gets all indices of a specified string inside this text.
Finds {pattern} into this string and returns Inumerable value.
#### When to use it?
When you want to find a string into another string in your applications.

#### Example:
```csharp
"abcde".AllIndicesOf("b").ToList();
returns:
Count = 1
    [0]: 1
```
```csharp
"abcde".AllIndicesOf("b").ToList()[0]; //returns 1
```
```csharp
"abcbe".AllIndicesOf("b").ToList();
returns:
Count = 2
    [0]: 1
    [1]: 3
```
```csharp
"abcbe".AllIndicesOf("a").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"abcba".AllIndicesOf("a").ToList();
returns:
Count = 2
    [0]: 0
    [1]: 4
```
```csharp
"aaaaaa".AllIndicesOf("a").ToList();
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
"aaaaaa".AllIndicesOf("aa").ToList();
returns:
Count = 5
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
    [4]: 4
 ```
 ```csharp
"aaaaaa".AllIndicesOf("aaa").ToList();
returns:
Count = 4
    [0]: 0
    [1]: 1
    [2]: 2
    [3]: 3
```
```csharp
"aaaaaa".AllIndicesOf("aaaa").ToList();
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
"aaaaaa".AllIndicesOf("aaaaaa").ToList();
returns:
Count = 1
    [0]: 0
```
```csharp
"aaaaaa".AllIndicesOf("aaaaaaa").ToList(); //returns Count = 0
```

 # AsDirectory()
Converts this path into a directory object.
#### When to use it?
When you want to convert this path into a directory object.
#### Example:

```csharp    
var tempFolder = Path.GetTempPath().AsDirectory().GetOrCreateSubDirectory("Temp7Zip-" + Guid.NewGuid());
```

# AsFile()
Converts this path into a file object.

#### When to use it?
When you want to convert this path into a file object.
#### Example:
```csharp         
FileInfo fi1;
string pathString = @"C:\sample.txt";
fi1 = pathString.AsFile();

//Create a file to write to.
using (StreamWriter sw = fi1.CreateText()) 
{
    sw.WriteLine("Hello");
    sw.WriteLine("And");
    sw.WriteLine("Welcome");
}	

//Open the file to read from.
using (StreamReader sr = fi1.OpenText()) 
{
    string s = "";
    while ((s = sr.ReadLine()) != null) 
    {
        Console.WriteLine(s);
    }
}

```

# AsUri()
 Converts this path into a Uri object.
A URI can be further classified as a locator, a name, or both. The term “Uniform Resource Locator” (URL) refers to the subset of URIs that, in addition to identifying a resource,
provide a means of locating the resource by describing its primary access mechanism (e.g., its network “location”).
#### When to use it?
When you want to convert this path into a Uri object. For example if you want to create a new folder in a directory. See example code.
#### Example:
```csharp    
 var html = $"https://www.nuget.org/packages/{package}".AsUri().Download()
 ```

# CapitaliseFirstLetters()
Capitalises first characters of each worlds in this string.
#### When to use it?
When you want to Capitalises first characters of each worlds in this string in your application codes.
#### Example:
```csharp           
"hello world".CapitaliseFirstLetters(); // returns "Hello World"
"hello WOLRD".CapitaliseFirstLetters(); // returns "Hello WOLRD"        
```

# Chop({separator},{chopSize})
Chops a list into same-size smaller lists. For example:
new int[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 }.Chop(5)
will return: { {1,2,3,4,5}, {6,7,8,9,10}, {11,12,13,14,15}, {16} }
#### When to use it?
If you want to Chops a list into same-size smaller lists in your application codes.
#### Example:
```csharp          
Int32[] intstr= {1,2,3,4,5,6,7};
IEnumerable<IEnumerable<Int32>> items3;
items3 = intstr.Chop<int>(3); // returns {1,2,3},{4,5,6},{7}
```

# Concat({newitem})
Concatenates two sequences.
#### When to use it?
If you want to concatenate two sequences in your application codes.
#### Example:
```csharp          
IEnumerable<string> items = new string[] {"msg","item1","item2" }.Concat("new string");
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

# ContainsAny({keywords},{caseSensitive})
Determines whether this text contains any of the specified keywords.
If the keywords list contains a null or empty string, it throws an exception. If you wish to ignore those, use .Trim() on your keywords list.
{keywords} is an `IEnumerable<string>` of the specified keywords which you want to find in this string.
{caseSensitive} default value is `true`.

#### When to use it?
When you want to determine whether this text contains any of the specified keywords.
#### Example:
```csharp 
IEnumerable<string> items = new string[] {"msg","item1","item2" };
"abc".ContainsAny(items); // returns false
"item1".ContainsAny(items); // returns true
"Item1".ContainsAny(items); // returns false
"Item1".ContainsAny(items,false); // returns true

IEnumerable<string> items2 = new string[] {};
"abc".ContainsAny(items2); // throws error : "contains a null or empty string element."
```

# ContainsWholeWord({string word},{caseSensitive})
Returns if a specified WHOLE WORD ({string word}) is found in this text. It skips occurances of the word with characters or digits attached to it.
#### When to use it?
If you want to determine whether a specified WHOLE WORD is found in this text. It skips occurances of the word with characters or digits attached to it in your application codes.
#### Example:
```csharp          
"sample string".ContainsWholeWord("tri"); // returns false
"sample string".ContainsWholeWord("string"); // returns true
"sample string".ContainsWholeWord("sample"); // returns true
"sample string".ContainsWholeWord("sample "); // returns true
"Sample string".ContainsWholeWord("sample",true); // returns false
"Sample string".ContainsWholeWord("sample",false); // returns true
```

# CreateHash({salt})
Creates a hash of a specified clear text with a mix of MD5 and SHA1. The output string has 27 characters.
In cryptography, a `salt` is random data that is used as an additional input to a one-way function that "hashes" data, 
a password or passphrase.
`Salt`s are used to safeguard passwords in storage. 
The default value of {salt} is `null`.
#### When to use it?
When you need to create a hash of a specified clear text with a mix of MD5 and SHA1.
It is really useful when you want to encrypt data.

#### Example:
```csharp           
"d".CreateHash(); // returns "H/I5OEZ9jPvv/81k6H8B4p9IqRk"
"d".CreateHash("a"); // returns "+x6vkOSmHX0GJn2TbF46+7axQJg"
"".CreateHash(); // returns "K0Ac+4mQlCeJuGbPVGJV2DxsWbc"
"".CreateHash(""); // returns "K0Ac+4mQlCeJuGbPVGJV2DxsWbc"
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAA".CreateHash(""); // returns "AFVSmNFlH3lzCPFjYhNwSnjJWtY"
```
# CreateMD5Hash({asHex})
Creates MD5 hash of this text.
The default value of {salt} is `false`. It Specifies whether a hex-compatible string is expected.
The MD5 message-digest algorithm is a widely used hash function producing a 128-bit hash value. 
Although MD5 was initially designed to be used as a cryptographic hash function, it has been found to suffer from extensive vulnerabilities.
#### When to use it?
When you need to create a MD5 of a specified clear text.
It is really useful when you want to encrypt data.

#### Example:
```csharp           
"".CreateMD5Hash(); // returns "1B2M2Y8AsgTpgAmY7PhCfg=="
"".CreateMD5Hash(false); // returns "1B2M2Y8AsgTpgAmY7PhCfg=="
"ABCDEF".CreateMD5Hash(); // returns "iCekESKlAouYCMe/hLn89g=="
"ABCDEF".CreateMD5Hash(false); // returns "iCekESKlAouYCMe/hLn89g=="
```
# CreateSHA1Hash({asHex})
Creates SHA1 hash of this text.
The default value of {salt} is `null`. It Specifies whether a hex-compatible string is expected.
The MD5 message-digest algorithm is a widely used hash function producing a 128-bit hash value. 
Although MD5 was initially designed to be used as a cryptographic hash function, it has been found to suffer from extensive vulnerabilities.
#### When to use it?
When you need to create a MD5 of a specified clear text.
It is really useful when you want to encrypt data.

#### Example:
```csharp           
"ABCDEF".CreateSHA1Hash(); // returns "lwCTZ4sYISf2C7UbivLJTVOeyjo"
"ABCDEF".CreateSHA1Hash(); // returns "lwCTZ4sYISf2C7UbivLJTVOeyjo"
"".CreateSHA1Hash(); // returns "2jmj7l5rSw0yVb/vlWAYkK/YBwk"
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
# EnsureEndsWith({expressionstring},{caseSensitive})
Ensure that this string object ends with a specified other one.
If it does not, then it prepends that and return the combined text.
#### When to use it?
If you want to determine whether that this string object ends with a specified other one in your application codes.
#### Example:
```csharp          
"Sample string".EnsureEndsWith("ing"); // returns "Sample string"
"Sample string".EnsureEndsWith("str"); // returns "Sample stringstr"
"Sample string".EnsureEndsWith("Sample"); // returns "Sample stringSample"
"Sample string".EnsureEndsWith("ing",true); // returns "Sample string"
"Sample string".EnsureEndsWith("Ing",true); // returns "Sample stringIng"
"".EnsureEndsWith("Ing",true); // returns "Ing"
"Sample string".EnsureEndsWith("",true); // returns "Sample string"
"".EnsureEndsWith("",true); // returns ""
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

# EnsureStartsWith({expressionstring},{caseSensitive})
Ensure that this string object starts with a specified other one.
If it does not, then it prepends that and return the combined text.
#### When to use it?
If you want to determine whether that this string object starts with a specified other one in your application codes.
#### Example:
```csharp          
"Sample string".EnsureStartsWith("Sam"); // returns "Sample string"
"Sample string".EnsureStartsWith("am"); // returns "amSample string"
"Sample string".EnsureStartsWith("am",true); // returns "amSample string"
"Sample string".EnsureStartsWith("Am",true); // returns "AmSample string"
"Sample string".EnsureStartsWith("",true); // returns "Sample string"
"".EnsureStartsWith("Am",true); // returns "Am"
"".EnsureStartsWith(""); // returns ""
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

# GetSingleMatchedValueOrDefault()
Returns the only matched string in the given text using this Regex pattern. 
Returns null if more than one match found.
#### When to use it?
When you want to return the only matched string in the given text using a Regex pattern in your application code.
#### Example:
```csharp           
Regex regex = new Regex(@"^[0-9]+$");
regex.GetSingleMatchedValueOrDefault("155555"); // returns 155555 because all characters are digit and the regex needs digits.
regex.GetSingleMatchedValueOrDefault("155555B"); // returns `null` because all characters should be digit.
regex.GetSingleMatchedValueOrDefault("ABC"); // returns `null` because all characters should be digit.
regex.GetSingleMatchedValueOrDefault(""); // returns `null` because all characters should be digit.
regex.GetSingleMatchedValueOrDefault("!"); // returns `null` because all characters should be digit.
```


# GetUtf8WithSignatureBytes()
Gets the UTF8-with-signature bytes of this text.
It returns `Unicode` code of all characters of this text. Also, it returns Utf-8 signature code: BOM.
The UTF-8 BOM is a sequence of bytes (EF BB BF) that allows the reader to identify a file as being encoded in UTF-8. Normally, 
the BOM is used to signal the endianness of an encoding, but since endianness is irrelevant to UTF-8, the BOM is unnecessary.
#### When to use it?
When you want to get the UTF8-with-signature bytes of this text in your application codes.
#### Example:
```csharp 
"abc".GetUtf8WithSignatureBytes();
Returns: 
{byte[6]}
    [0]: 239 // BOM
    [1]: 187 // BOM
    [2]: 191 // BOM
    [3]: 97 // a Unicode  
    [4]: 98 // b Unicode
    [5]: 99 // c Unicode

```
# HasMany()
Returns true if this collection has more than one item.
#### When to use it?
When you want to know whether an IEnumerable has any item or not in your application code.
It returns true if this value has any item.
#### Example:
```csharp           
IEnumerable<string> items = new string[] {"msg","item1","item2" };
IEnumerable<string> items2 ;
items.HasMany(); // returns true because items has 3 items.
items2.HasMany(); // returns false because items2 has no item.
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


# JavascriptEncode()
Returns valid JavaScript string content with reserved characters replaced by encoded literals.
> '\'' replaced by '\"'
> NewLine replaced by "\\n"
#### When to use it?
When you want to return valid JavaScript string content with reserved characters replaced by encoded literals in your application code.
#### Example:
```csharp           
"<data>abc23123</data>\r\n".JavascriptEncode(); // returns "<data>abc23123</data>\\n"
"<data>\'abc23123</data>\r\n".JavascriptEncode(); // returns "<data>\\x27abc23123</data>\\n"
```

# KeepReplacing({original},{substitute},{caseSensitive})
Replaces all occurances of a specified phrase to a substitude, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
{caseSensitive} determines whether case sensitive of method is important or not.
#### When to use it?
When you want to replace some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".KeepReplacing("xa","pe");// returns "Sample Epemple"
"Sample Example".KeepReplacing("a","p");// returns "Spmple Expmple"
"Sample Example".KeepReplacing(" ","");// returns "SampleExample"
"Sample Example".KeepReplacing("a","p",false);// returns "Spmple Expmple", caseSensitive is false
"Sample Example".KeepReplacing("A","P",true);// returns "Sample Example", caseSensitive is true
"Sample Example".KeepReplacing("E","p",true);// returns "Sample pxample",caseSensitive is true
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
# OnlyWhen({condition})
Gets this same string when a specified condition is True, otherwise it returns empty string.
#### When to use it?
When you want to get this same string when a specified condition is True, otherwise it returns empty string in your application codes.
#### Example:
```csharp 
"Sample Example".OnlyWhen(true);// returns "Sample Example"
"Sample Example".OnlyWhen(false);// returns ""
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
# OrEmpty()
If it's null, it return empty string. Otherwise it returns this.
#### When to use it?
When you do not want to return `null` in your application code.
#### Example:
```csharp           
"".OrEmpty(); // returns ""
"Example String".OrEmpty(); // returns "Example String"
```
# OrNullIfEmpty()
Returns this string. But if it's String.Empty, it returns NULL.
#### When to use it?
When you want to return this string, but if it's String.Empty, it returns NULL in your application codes.
#### Example:
```csharp          
"Sample String".OrNullIfEmpty(); // returns "Sample String"
"".OrNullIfEmpty(); // returns null
" ".OrNullIfEmpty(); // returns " "
```

# Remove({firstSubstringsToRemove},{otherSubstringsToRemove},{caseSensitive})
Removes the specified substrings from this string object.
{caseSensitive} determines whether case sensitive of method is important or not.
#### When to use it?
When you want to Remove some substring from  another string in your application.
#### Example:
```csharp 
"Sample Example".Remove("Sample "); //returns "Example"
"Sample Example".Remove("a"); //returns "Smple Exmple"
"Sample Example".Remove("a","b"); //returns "Smple Exmple"
"Sample Example".Remove("a","x"); //returns "Smple Emple"
"Sample Example".Remove("a","x","E"); //returns "Smple mple"
"Sample Example".Remove("a","x","E",caseSensitive: false); //returns "Smple mple", caseSensitive is false
"Sample Example".Remove("A","X","E",caseSensitive: true); //returns "Sample xample", caseSensitive is true
```

# RemoveHtmlTags()
Removes all Html tags from this html string.
#### When to use it?
It is useful when you want to show context of a Html document.
#### Example:
```csharp   
"<Br><b><span>Hello World</span></b></br>".RemoveHtmlTags();// returns "Hello World"
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
# ReplaceWholeWord({stringword},{replacement},{caseSensitive})
It will replace all occurances of a specified WHOLE WORD and skip occurances of the word with characters or digits attached to it.
#### When to use it?
It will replace all occurances of a specified WHOLE WORD and skip occurances of the word with characters or digits attached to it in your application codes.
#### Example:
```csharp          
"sample string".ReplaceWholeWord("string","Word"); // returns "sample Word"
"sample string".ReplaceWholeWord("string","Word",false); // returns "sample Word"
"sample string".ReplaceWholeWord("string","Word",true); // returns "sample Word"
"sample string".ReplaceWholeWord("String","Word",true); // returns "sample string"
"sample string".ReplaceWholeWord(" ","Word",true); // returns "sampleWordstring"
"sample string".ReplaceWholeWord(" ","",true); // returns "samplestring"
"sample string".ReplaceWholeWord("","",true); // returns "sample string"
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
# SeparateAtUpperCases()
Inserts a space after each uppercase characters in this string.
#### When to use it?
If you want to inserts a space after each uppercase characters of a string in your application codes.
#### Example:
```csharp          
"Sample String".SeparateAtUpperCases(); // returns "sample  string"
"Sample string".SeparateAtUpperCases(); // returns "sample string"
"aample string".SeparateAtUpperCases(); // returns "aample string"
"Aample Sample string".SeparateAtUpperCases(); // returns "aample  sample string"
"Aample Sample String".SeparateAtUpperCases(); // returns "aample  sample  string"
```

# Split({chunkSize})
Splits this string into some `IEnumerable` strings which have {chunkSize} charachers. If {chunkSize} is 1, it returns all this string.
#### When to use it?
When you want to divide a string into other strings.
#### Example:
```csharp           
IEnumerable<string> items ;
items = "abcdefg hij".Split(1); // returns items[0] = "abcdefg hij"
items = "abcdefg hij".Split(4); // returns items[0] = "abcd" , items[1]="efg " , items[2]="hij"
items = "abc".Split(2); // returns items[0] = "ab" , items[1]="c"
items = "abcdefg hij".Split(12); // returns items[0] = "abcdefg hij"
```    

# ToBase64String()
Converts this array of bytes to a Base64 string.
Base64 is a group of similar binary-to-text encoding schemes that represent binary data in an ASCII string format by translating it into a radix-64 representation. 
The term Base64 originates from a specific MIME content transfer encoding.
#### When to use it?
Many people are not aware that images can be inlined into html code. The method in which this is done is called base64 encoding.
Base64 encoded images become part of the html and displays without loading anything instead of a web browser having to download the image.
For more information you can see https://varvy.com/pagespeed/base64-images.html 
#### Example:
```csharp 
new byte[1].ToBase64String(); //returns "AA=="
new byte[2].ToBase64String(); //returns "AAA="
new byte[3].ToBase64String(); //returns "AAAA"
new byte[100].ToBase64String(); //returns "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="
new byte[0].ToBase64String(); //returns ""
new byte[50].ToBase64String(); //returns "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
```

# StartsWith({otherstring},{StringComparison})
 Gets whether this string item begins with any of the specified items.
`StringComparison` enum influences string comparisons. It contains several constants. 
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

# StartsWithAny({Args},{caseSensitive})
Gets whether this string item begins with any of the specified items{Args}.
{caseSensitive} determines whether case sensitive of method is important or not.
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
"there".StartsWithAny("The",false); //returns true
"there".StartsWithAny("The",true); //returns false, caseSensitive is true
"there".StartsWithAny("er","th"); //returns true
"there".StartsWithAny("the","th"); //returns true
"Sample string".StartsWithAny("s","Sa","Sam"); //returns true
"Sample string".StartsWithAny("s","Sa","am"); //returns true
"Sample string".StartsWithAny("an","as","am"); //returns false

```

# Substring({fromindex},{toText},{inclusive},{casesensitive})
> Another override : Substring({from},{to},{inclusive},{casesensitive})

Gets a peice of this string from specific start to specific end place.
{fromindex} is an `Integer` argument that determines substring is started from which character index. The first character of this string is started from Zero.
{toText} is a string that determines the end of the output string. If this method finds {ToText} in the this string, it returns all characters before it. Otherwise, it returns `Empty`.
{inclusive} determines whether the output string contains {from} and {to} strings or not. Default value is `false`.
#### When to use it?
When you want to get a substring of a string with special start and end in your application code.
#### Example:
```csharp           
"abcde".Substring(2,"d"); // returns "c"
"abcde".Substring(0,"d"); // returns "abc"
"abcde".Substring(-1,"d"); // returns ""
"abcde".Substring(0,"g"); // returns ""
"abcde".Substring(1,"g"); // returns ""
"abcde".Substring(0,"e"); // returns "abcd"
"abcde".Substring(1,"e"); // returns "bcd"
"abcde".Substring(1,"d"); // returns "bc"
"abcde".Substring("b","e",false); // returns "cd"
"abcde".Substring("b","e",true); // returns "bcde"
"abcde".Substring("b","e",true,false); // returns "bcde"
"abcde".Substring("B","e",true,false); // returns "bcde"
"abcde".Substring("B","e",true,true); // returns ""
"".Substring("B","e",true,true); // throws Exception thrown: 'System.ArgumentOutOfRangeException' in mscorlib.dll
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
#### Other Overrides
- TrimEnd({unnecessaryText},{IscaseSensitive})
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

- TrimEnd({unnecessaryChar[]})
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

# TrimStart({textToTrim})
Removes all leading occurrences of a set of characters specified in this string.
The string that remains after all occurrences of characters in this string are removed from the start of the current string.
#### When to use it?
When you want to remove all leading occurrences of a set of characters specified of a string in your application codes.
#### Example:
```csharp           
"Sample String".TrimStart("el"); // returns "Sample String"
"Sample String".TrimStart("le"); // returns "Sample String"
"Sample String".TrimStart("Sa"); // returns "mple String"
"Sample String".TrimStart("ing"); // returns "Sample String"
"Sample String".TrimStart("sample"); // returns "Sample String"
"Sample String".TrimStart("sample "); // returns "Sample String"
"Sample String".TrimStart("sa"); // returns "Sample String"
"Sample String".TrimStart("Sa"); // returns "mple String"
"Sample String".TrimStart("Sample"); // returns " String"
"Sample String".TrimStart("Sample "); // returns "String"
"abbaa".TrimStart("a").ToString(); //returns "bbaa"
"abbaa".TrimStart("A").ToString(); //returns "abbaa"
"abbaa".TrimStart("A",false).ToString(); //returns "bbaa"
"abbaa".TrimStart("AB",false).ToString(); //returns "baa"
"abbaa".TrimStart("AB").ToString(); //returns "abbaa"
"abbaa".TrimStart("abb").ToString(); //returns "aa"
"Sample String".TrimStart("sample",false); // returns " String"
"Sample String".TrimStart("sample",true); // returns " Sample String"
```        
#### Other Overrides
- TrimStart({searchString},{IscaseSensitive})
Removes the specified text from the start of this string instance.
Finds {searchString} into this string and removes it from left side of this string. Default value is `true`.
If {IscaseSensitive} is false, `CaseSensitive` is not important. 
#### When to use it?
When you want to find a string into another string and remove it in your applications.

#### Example:
```csharp
"Sample String".TrimAfter(""); // returns ""
"".TrimStart("Sample "); // returns ""

```
- TrimStart({char[]})  
You can give a list of characters and this method removes all of them.

```csharp
"abbaa".TrimStart('a','c').ToString(); //returns "bbaa"
"abcdef".TrimStart('a','c').ToString(); //returns "bcdef"
"abcdef".TrimStart('a','b').ToString(); //returns "cdef"
"abcdef".TrimStart('a','b','c').ToString(); //returns "def"
"abcdef".TrimStart('c','b','a').ToString(); //returns "def"
```

# ToBytes({Encoding})
Converts this Base64 string to an array of bytes.
Base64 is a group of similar binary-to-text encoding schemes that represent binary data in an ASCII string format by translating it into a radix-64 representation. 
The term Base64 originates from a specific MIME content transfer encoding.
You can set the encoding which you want to return based on it:
- ASCII
- BigEndianUnicode
- Default
- Unicode
- UTF32
- UTF7
- UTF8

#### When to use it?
Many people are not aware that images can be inlined into html code. The method in which this is done is called base64 encoding.
Base64 encoded images become part of the html and displays without loading anything instead of a web browser having to download the image.
For more information you can see https://varvy.com/pagespeed/base64-images.html 
#### Example:
```csharp 
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==".ToBytes(); //returns {byte[97]}
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=".ToBytes(); // returns {byte[98]}
```
# ToCamelCaseId()
Returns valid CamelCase JavaScript or C# string content.
`camelCase` (Camelcase, CamelCase, camel case, camel caps or medial capitals) is the practice of writing compound words or phrases such that each word or abbreviation begins with a capital letter. Camel case may start with a capital or, especially in programming languages, with a lowercase letter.
`Pascalcase` is a subset of Camel Case where the first letter is capitalized.
That is, `userAccount` is a camel case and `UserAccount` is a Pascal case.
#### When to use it?
The conventions of using these are different. You use camel case for variables and Pascal case for Class names or Constructors.
It is easy to remember. Pascal is a proper noun so capitalize the first letter.
camel is a common noun, so you do not capitalize the first letter.
#### Example:
```csharp           
"useracountId".ToCamelCaseId(); // returns "useracountId"
"user acount Id".ToCamelCaseId(); // returns "useracountId"
```
# ToCountString({count},{zeroQualifier})
Inserts a "s" in the end of this string if {count} greater than zero.
{zeroQualifier} is a string that if the {count} is zero, it is added to the output string.
#### When to use it?
If you want to inserts a "s" in the end of this string if {count} greater than zero in your application codes.
#### Example:
```csharp          
"".ToCountString(1); // throws Exception("'name' cannot be empty for ToCountString().")
"".ToCountString(2); // throws Exception("'name' cannot be empty for ToCountString().")
"apple".ToCountString(0); // throws ArgumentException("count should be greater than or equal to 0.")
"apple".ToCountString(1); // returns "1 apple"
"apple".ToCountString(2); // returns "2 apples"
"fox".ToCountString(1,"Zero"); // returns "1 fox"
"fox".ToCountString(2,"Zero"); // returns "2 foxs"
"fox".ToCountString(0,"Zero"); // returns "Zero fox"
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
# ToJsonText()
Escapes all invalid characters of this string to it's usable as a valid json constant.
> It replaces "\\" with "\\\\".
> It replaces "\r" with "\\r".
> It replaces "\n" with "\\n.
> It replaces "\t" with "\\t".
> It replaces "\", "\\\".
#### When to use it?
When you want to escape all invalid characters of this string to it's usable as a valid json constant in your application code.
#### Example:
```csharp           
"\r\n\t".ToJsonText(); // returns "\\r\\n\\t"
"\r\n\t\\".ToJsonText(); // returns "\\r\\n\\t\\\\"
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
# ToLiteralFromPascalCase()
Returns natural English literal text for a specified pascal case string value.
#### When to use it?
When you want to return natural English literal text for a specified pascal case string value in your application codes.
#### Example:
```csharp 
"ThisIsSomething".ToLiteralFromPascalCase();// returns "This is something"
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

# ToPascalCaseId()
Returns valid PascalCase JavaScript or C# string content.
`camelCase` (Camelcase, CamelCase, camel case, camel caps or medial capitals) is the practice of writing compound words or phrases such that each word or abbreviation begins with a capital letter. Camel case may start with a capital or, especially in programming languages, with a lowercase letter.
`Pascalcase` is a subset of Camel Case where the first letter is capitalized.
That is, `userAccount` is a camel case and `UserAccount` is a Pascal case.
#### When to use it?
The conventions of using these are different. You use camel case for variables and Pascal case for Class names or Constructors.
It is easy to remember. Pascal is a proper noun so capitalize the first letter.
camel is a common noun, so you do not capitalize the first letter.
#### Example:
```csharp           
"useracountId".ToPascalCaseId(); // returns "UseracountId"
"user acount Id".ToPascalCaseId(); // returns "UseracountId"
```
# ToPlural()
Returns the plural form of this word.
#### When to use it?
When you want to know  the plural form of a word in your application codes.
#### Example:
```csharp          
"leaf".ToPlural(); // returns "Leaves"
"fox".ToPlural(); // returns "foxes"
"bus".ToPlural(); // returns "buses"
"quiz".ToPlural(); // returns "quizs"
"city".ToPlural(); // returns "cities"
"man".ToPlural(); // returns "Men"
"dish".ToPlural(); // returns "dishes"
"knife".ToPlural(); // returns "Knives"
"hero".ToPlural(); // returns "Heroes"
"stereo".ToPlural(); // returns "stereos"
"video".ToPlural(); // returns "videos"
"sheep".ToPlural(); // returns "Sheep"
"deer".ToPlural(); // returns "Deer"
"fish".ToPlural(); // returns "Fish"
"series".ToPlural(); // returns "Series"
"species".ToPlural(); // returns "Species"
"wolf".ToPlural(); // returns "Wolves"
"car".ToPlural(); // returns "cars"
"cactus".ToPlural(); // returns "cacti"
"syllabus".ToPlural(); // returns "syllabi"
```
# ToProperCase()
Capitalises the first letter and lower-cases the rest.
#### When to use it?
When you want to capitalise the first letter and lower-cases the rest of a string in your application codes.
#### Example:
```csharp          
"Sample String".ToProperCase(); // returns "Sample string"
"sample string".ToProperCase(); // returns "Sample string"
"SAMPLE STRING".ToProperCase(); // returns "Sample string"
"sAMPLE STRING".ToProperCase(); // returns "Sample string"
```        

# ToSafeFileName({replacement})
Detects the characters which are not acceptable in File System and replaces them with a hyphen.
{replacement} is the character with which to replace invalid characters in the name. The default value is '-'.

#### When to use it?
When you want to detect the characters which are not acceptable in File System and replaces them with a hyphen. in your application codes.
Invalid characters are '<', '>', ':', '"', '/', '\\', '|', '?', '*'.
#### Example:
```csharp          
"fff".ToSafeFileName(); // returns "fff"
"fff<>/*".ToSafeFileName(); // returns "fff-"
"fff<>.txt".ToSafeFileName(); // returns "fff-.txt"
"fff***.txt".ToSafeFileName(); // returns "fff-.txt"
"Sam/ple*.txt".ToSafeFileName(); // returns "Sam-ple-.txt"
"*Sam/ple*.txt".ToSafeFileName(); // returns "-Sam-ple-.txt"
"/Sam/ple*.txt".ToSafeFileName(); // returns "-Sam-ple-.txt"
"Sam/ple*.txt".ToSafeFileName(); // returns "Sam-ple-.txt"
"\\Sam/ple*.txt".ToSafeFileName(); // returns "-Sam-ple-.txt"
"\\Sam/ple*.txt".ToSafeFileName('@'); // returns "@Sam@ple@.txt"
"\\Sam/ple*.txt".ToSafeFileName('_'); // returns "_Sam_ple_.txt"

```

# ToSimplifiedSHA1Hash()
Gets a SHA1 hash of this text where all characters are alpha numeric.
#### When to use it?
When you want to get a SHA1 hash of this text where all characters are alpha numeric in your application code.
#### Example:
```csharp           
"dddd".ToSimplifiedSHA1Hash(); // returns "AUZOFhbjdXGDAzFUWwdFFTMQYU"
"".ToSimplifiedSHA1Hash(); // returns "2jmj7l5rSw0yVbvlWAYkKYBwk"
" ".ToSimplifiedSHA1Hash(); // returns "uFjLKCYXwlW2WAhXI6E0cz5CcY"
"ABCDEFG".ToSimplifiedSHA1Hash(); // returns "k75GEsQdI68YkdrFQ1TVzbxOM"
"123456".ToSimplifiedSHA1Hash(); // returns "fEqNCco3Yq9h5ZUglD3CZJT4lBs"
"abc".ToSimplifiedSHA1Hash(); // returns "qZkNkcGgWq6PiVxeFDCbJzQ2J0"
"!".ToSimplifiedSHA1Hash(); // returns "Crgxisr25njdAuK1w0PtQREbOT0"
"".ToSimplifiedSHA1Hash(); // returns "2jmj7l5rSw0yVbvlWAYkKYBwk"
" ".ToSimplifiedSHA1Hash(); // returns "uFjLKCYXwlW2WAhXI6E0cz5CcY"
```       

# ToString({separator},{lastSeparator})
It converts an object to its string representation so that it is suitable for display.
{separator} is a string which represents after all characters of this string.
{lastSeparator} is a string which represents after the last character of this string.
#### When to use it?
If you want to convert an object to its string representation so that it is suitable for display in your application codes.
#### Example:
```csharp          
"Sample String".ToString(" "); // returns "S a m p l e   S t r i n g"
"Sample String".ToString(","); // returns "S,a,m,p,l,e, ,S,t,r,i,n,g"
"Sample String".ToString("","-"); // returns "Sample Strin-g"
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


# TrimAfter({stringsearch},{trimPhrase},{caseSensitive})
Trims all text after the specified search phrase.
If {trimPhrase} is `true`, only phrases are returned instead of all characters. The defalt value is `false `.
If this string or {stringsearch} are `Empty`, it returns `Empty` string.
#### When to use it?
When you want to trim all text after the specified search phrase in your application codes.
#### Example:
```csharp           
"Sample String".TrimAfter("le"); // returns "Samp"
"Sample String".TrimAfter("el"); // returns "Sample String"
"Sample String".TrimAfter("le",caseSensitive: false); // returns "Samp"
"Sample String".TrimAfter("le",caseSensitive: true); // returns "Samp"
"Sample String".TrimAfter("Le",caseSensitive: true); // returns "Sample String"
"Sample String".TrimAfter("Le",caseSensitive: false); // returns "Samp"
"Sample String".TrimAfter("Le",caseSensitive: false,trimPhrase: false); // returns "Samp"
"Sample String".TrimAfter("Le",caseSensitive: false,trimPhrase: true); // returns " Sample"
"Sample String".TrimAfter(""); // returns ""

```        
# TrimBefore({stringsearch},{trimPhrase},{caseSensitive})
Trims all text before the specified search phrase.
If {trimPhrase} is `true`, only phrases are returned instead of all characters. The defalt value is `false `.
If this string or {stringsearch} are `Empty`, it returns `Empty` string.
{caseSensitive} determines whether case sensitive of method is important or not.
#### When to use it?
When you want to trim all text before the specified search phrase in your application codes.
#### Example:
```csharp           
"Sample String".TrimBefore("le"); // returns "le String"
"Sample String".TrimBefore("el"); // returns "Sample String"
"Sample String".TrimBefore("le",false); // returns "le String"
"Sample String".TrimBefore("le",true); // returns "le String"
"Sample String".TrimBefore("Le",true); // returns "Sample String"
"Sample String".TrimBefore("Le",false); // returns "le String"
"Sample String".TrimBefore("Le",false,false); // returns "le String" caseSensitive is false
"Sample String".TrimBefore("Le",false,true); // returns " String" caseSensitive is true
"".TrimBefore("Le"); // returns ""
"Sample String".TrimBefore(""); // returns ""
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

# Unless({unwantedText},{caseSensitive})
Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
{caseSensitive} determines whether case sensitive of method is important or not.
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
"Example".Unless("example",false); //returns "", caseSensitive is false
"Example".Unless("example",true); //returns "Example", caseSensitive is true
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

 # XmlDecode()
Gets the Xml Decoded version of this text.
It replaces "&amp;" with "&"
It replaces "&lt;" with "<"
It replaces "&gt;" with ">"
It replaces "&quot;"" with "\"
It replaces "&apos;" with "'"

#### When to use it?
When you need to encode an XML document and pass it through to a string parameter on a web service. It then needs to be decoded at the other end.

 # XmlEncode()
Gets the Xml Encoded version of this text.
- It replaces "&" with "&amp;"
- It replaces "<" with "&lt;"
- It replaces ">" with "&gt;"
- It replaces "\"" with "&quot;"
- It replaces "'" with "&apos;"

#### When to use it?
When you need to encode an XML document and pass it through to a string parameter on a web service. It then needs to be decoded at the other end.

#### Example:

```csharp    
var r = new StringBuilder();

r.AppendLine(@"<Row>");

foreach (var c in Columns)
{
    r.AddFormattedLine("<Data ss:Type=\"String\">{0}</Data>", c.HeaderText.XmlEncode());
}
```

# XmlEscape()
Returns a string value that can be saved in xml.
> It replaces "&" with "&amp;"
> It replaces "<" with "&lt;"
> It replaces ">" with "&gt;"
> It replaces "\"" with "&quot;"
> It replaces "'" with "&apos;"
#### When to use it?
When you want to pass a XML value through your WEB Api in your application code.
#### Example:
```csharp           
"<data>abc</data>".XmlEscape(); // returns ""&lt;data&gt;abc&lt;/data&gt;"
```
# XmlUnescape()
Returns a string value without any xml-escaped characters.
#### When to use it?
When you want to get a XML value through your WEB Api in your application code.
#### Example:
```csharp           
"&lt;data&gt;abc&lt;/data&gt;".XmlEscape(); // returns "<data>abc</data>"
```
