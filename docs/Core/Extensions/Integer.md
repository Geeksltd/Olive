

# Integer Extension Methods
>`Integer`  extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.
`Integer` is a 64 bit type that its range is between -2,147,483,648 to 2,147,483,647.

# AsPercentageOf({total}, {multiplyBy100}, {roundTo})
Return this value as a percentages the of the given total.
If you want to calculate based on 100, set {multiplyBy100} `true`. Its default value is false.
You can round the output by setting {RoundTo} parameter.

#### When to use it?
When you want to return an `Integer` value as a percentages the of the given total in your applications.

#### Example:
```csharp
57.AsPercentageOf(100,true,0); //returns57
57.AsPercentageOf(50,true,0); //returns114
57.AsPercentageOf(50,true,1); //returns114
57.AsPercentageOf(50,true,2);; //returns114
57.AsPercentageOf(50,false,2); //returns1.14
57.AsPercentageOf(50,false,0); //returns1
57.AsPercentageOf(50,false,2); //returns1.14
57.AsPercentageOf(40,false,2); //returns1.43
57.AsPercentageOf(40,false,1); //returns1.4
57.AsPercentageOf(40,false,0); //returns1

(57).AsPercentageOf(100,true,0); //returns 57
(57).AsPercentageOf(2500,false,3); //returns 0.023
(57).AsPercentageOf(2500,true,3); //returns  2.29
(57).AsPercentageOf(2500,true,2); //returns  2.29
(57).AsPercentageOf(2500,true,1); //returns  2.3
(57).AsPercentageOf(2500,true,0); //returns  2
(67).AsPercentageOf(2500,true,0); //returns  3
```

# CompareTo({otherValue})
Compare two values and returns 0, 1 or -1. If this value is equal to {othervalue}, it returns 0. If this value is greater than {othervalue}, it returns 1. If this value is smaller than {othervalue}, it returns -1.

#### When to use it?
When you want to compare two `Integer` values in your applications.

#### Example:
```csharp
(120).CompareTo(120);// returns 0
(120).CompareTo(110);// returns 1
(120).CompareTo(130);// returns -1
(120).CompareTo(null);// returns 1

Int? x1,x2= null;
(x1).CompareTo(x2); // returns 0
(x1).CompareTo(100);// returns  -1
(100).CompareTo(x2);// returns  1
```

# Days()
Returns an Integer value as Day value.

#### When to use it?
When you want to convert an `Integer` value to Day object and calculate Minutes, Seconds and Milliseconds of it.
#### Example:
```csharp
104.Days().TotalHours ;//returns 2496 hours
10.Days().TotalHours  ;//returns 240 hours
10.Days().TotalMinutes;//returns 14400 minutes
10.Days().TotalSeconds;//returns 864000 seconds
10.Days().TotalMilliseconds;//returns 864000000 miliseconds

```

# Hours()
Returns an Integer value as `Hour` value.

#### When to use it?
When you want to convert an `Integer` value to `Hour` value and calculate Days ,Minutes, Seconds and Milliseconds of it.
#### Example:
```csharp
140.Hours().TotalDays ;//returns 5.833333333333333
140.Hours().TotalHours ;//returns 140
140.Hours().TotalMinutes;//returns 8400
140.Hours().TotalSeconds;//returns 504000
140.Hours().TotalMilliseconds;//returns 504000000

```

# LimitMax({maxValue})
Determines the maximum limitation of two values. If this value is smaller than {maxValue}, this value is returned, owherwise, {maxvalue} is returned.

#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(121).LimitMax(120); //returns  120
(121).LimitMax(122); //returns  121
```

# LimitMin({minValue})
Determines the minimum limitation of two values. If this value is greater than {minValue}, this value is returned, owherwise, {minvalue} is returned.

#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(121).LimitMin(122); //returns  122
(121).LimitMin(120); //returns  121
```

# LimitWithIn({minValue},{maxValue})
Determines the minimum and maximum limitation of two values. If this value is between {minValue} and {maxValue}, this value is returned. If this value is smaller than {minvalue}, {minvalue} is returned. If this value is greater than {maxvalue}, {maxvalue} is returned.

#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(122).LimitWithin(121,123);// returns 122
(124).LimitWithin(121,123);// returns 123
(120).LimitWithin(121,123);// returns 121
```

# Milliseconds()
Returns an Integer value as `Millisecond` value.

#### When to use it?
When you want to convert an `Integer` value to `Millisecond` value and calculate Days ,Hours, Minutes of it.
#### Example:
```csharp

160000000.Milliseconds().TotalDays;//returns 1.8518518518518519
160000000.Milliseconds().TotalHours;//returns 44.444444444444443
160000000.Milliseconds().TotalMinutes;//returns 2666.6666666666665
160000000.Milliseconds().TotalSeconds;//returns 160000
```
# Minutes()
Returns an Integer value as `Minutes` value.

#### When to use it?
When you want to convert an `Integer` value to `Minutes` value and calculate Days, Hours, Seconds and Milliseconds of it.

#### Example:
```csharp
1400.Minutes().TotalDays;//returns 0.97222222222222221
1600.Minutes().TotalDays;//returns 1.1111111111111112
1600.Minutes().TotalHours;//returns 26.666666666666664
1600.Minutes().TotalMinutes;//returns 1600
1600.Minutes().TotalSeconds;//returns 96000
1600.Minutes().TotalMilliseconds;//returns 96000000
```

# RoundUpToNearest({roundIntervals})
Rounds up to nearest value with the intervals specified.

#### When to use it?
When you want to want to round up an Integer value in your applications.

#### Example:
```csharp
(50).RoundUpToNearest(1); // returns   50 ( 1 * 50 = 50, so 50 returned)
(50).RoundUpToNearest(2); // returns   50 ( 2 * 50 = 50, so 50 returned)
(50).RoundUpToNearest(3); // returns   51 ( 3 * 17 = 51, so 51 returned)
(20).RoundUpToNearest(3); // returns   21 ( 3 * 7 = 21, so 21 returned)
(52).RoundUpToNearest(3); // returns   54 ( 3 * 18 = 54, so 54 returned)
(121).RoundUpToNearest(30); // returns 150( 3 * 50 = 150, so 150 returned)
```

# RoundDownToNearest({roundIntervals})
Rounds down to nearest value.

#### When to use it?
When you want to rounds down to nearest value in your applications.

#### Example:
```csharp
(50).RoundDownToNearest(1); // returns   50 ( 1 * 50 = 50, so 50 returned)
(50).RoundDownToNearest(2); // returns   50 ( 2 * 50 = 50, so 50 returned)
(50).RoundDownToNearest(3); // returns   51 ( 3 * 16 = 48, so 48 returned)
(20).RoundDownToNearest(3); // returns   21 ( 3 * 6 = 18, so 18 returned)
(52).RoundDownToNearest(3); // returns   54 ( 3 * 17 = 51, so 51 returned)
(121).RoundDownToNearest(30); // returns 150( 3 * 40 = 120, so 120 returned)
```


# Seconds()
Returns an Integer value as `Seconds` value.

#### When to use it?
When you want to convert an `Integer` value to `Second` value and calculate Days ,Hours, Minutes and Milliseconds of it.
#### Example:
```csharp

160000.Seconds().TotalDays;//returns  1.8518518518518519
160000.Seconds().TotalHours;//returns 44.444444444444443
160000.Seconds().TotalMinutes;//returns 2666.6666666666665
160000.Seconds().TotalMilliseconds;//returns 160000000
```

# ToBase32()
`Base32` is one of several base 32 transfer encodings. `Base32` uses a 32-character set comprising the twenty-six upper-case letters A–Z, and the digits 2–7.
`Base32` is primarily used to encode binary data, but `Base32` is also able to encode binary text like `ASCII`.
`Base32` is a notation for encoding arbitrary byte data using a restricted set of symbols that can be conveniently used by humans and processed by computers.
`Base32` consists of a symbol set made up of 32 different characters, as well as an algorithm for encoding arbitrary sequences of 8-bit bytes into the Base32 alphabet. 
Because more than one 5-bit Base32 symbol is needed to represent each 8-bit input byte, it also specifies requirements on the allowed lengths of Base32 strings (which must be multiples of 40 bits). 
The closely related Base64 system, in contrast, uses a set of 64 symbols.
#### Advantages:
**Base32 has a number of advantages over Base64:**

- The resulting character set is all one case, which can often be beneficial when using a case-insensitive filesystem, spoken language, or human memory.
- The result can be used as a file name because it cannot possibly contain the '/' symbol, which is the Unix path separator.
- The alphabet can be selected to avoid similar-looking pairs of different symbols, so the strings can be accurately transcribed by hand. (For example, the RFC 4648 symbol set omits the digits for one, eight and zero, since they could be confused with the letters 'I', 'B', and 'O'.)
- A result excluding padding can be included in a URL without encoding any characters.

**Base32 also has advantages over hexadecimal/Base16:**

`Base32` representation takes roughly 20% less space. (1000 bits takes 200 characters, compared to 250 for Base16)

#### Disadvantages:
`Base32` representation takes roughly 20% more space than Base64. Also, because it encodes 5 bytes to 8 characters (rather than 3 bytes to 4 characters), padding to an 8-character boundary is a greater burden on short messages.

#### When to use it?
Base32 encoding (and Base64) encoding is motivated by situations where you need to encode unrestricted binary within a storage or transport system that allow only data of a certain form such as plain text. 
Examples include passing data through URLs, XML, or JSON data, all of which are plain text sort of formats that don't otherwise permit or support arbitrary binary data.
#### Example:
```csharp
(500).ToBase32(); //returns {5Y}
(5000).ToBase32(); //returns{WKE}
(50000).ToBase32(); //returns{ATMT}
(500000).ToBase32(); //returns{5ED1}
(500123).ToBase32(); //returns{5ER8}
(0).ToBase32(); //returns{1}
(1).ToBase32(); //returns{A}

```

# ToFileSizeString({Unit},{round})
Emits a user readable file size (including units).
You can set the unit of this value by {Unit} and round the output by {round}.
#### When to use it?
When you want to convert an `Integer` values as Bytes in your applications.

#### Example:
```csharp
0.ToFileSizeString();//returns "0B"
12345678900.ToFileSizeString();//returns "11.5GB"
1.ToFileSizeString();//returns "11.5GB""1B"
1024.ToFileSizeString();//returns "11.5GB""1KB"
1024000.ToFileSizeString();//returns "11.5GB""1000KB"
10240000.ToFileSizeString();//returns "11.5GB""9.8MB"
1000000.ToFileSizeString();//returns "11.5GB""976.6KB"
1048576.ToFileSizeString();//returns "11.5GB""1MB"
1073741824.ToFileSizeString();//returns "11.5GB""1GB"

1024.ToFileSizeString("MB",0);//returns"0.0 MB"
1024.ToFileSizeString("KB",0);//returns"1024 B"
1024.ToFileSizeString("KB",1);//returns"1024 B"
1024.ToFileSizeString("MB",1);//returns"0.0 MB"
1073741824.ToFileSizeString();//returns"1GB"
1073741824.ToFileSizeString("MB",0);//returns"1024.0 MB"
1073741824.ToFileSizeString("KB",0);//returns"1024 MB"
1073741824.ToFileSizeString("B",0);//returns"1024 MB"
1073741824.ToFileSizeString("MB",1);//returns"1024.0 MB"
1073741824.ToFileSizeString("KB",1);//returns"1024 MB"
1048576.ToFileSizeString();//returns"1MB"
1048576.ToFileSizeString("MB",0);//returns"1.0 MB"
1048576.ToFileSizeString("MB",1);//returns"1.0 MB"
1048576.ToFileSizeString("KB",1);//returns"1024 KB"
1048576.ToFileSizeString("GB",1);//returns"1024 KB"
1073741824.ToFileSizeString("GB",1);//returns"1024 MB"
1073741824.ToFileSizeString("MB",1);//returns"1024.0 MB"
1073741824.ToFileSizeString("KB",1);//returns"1024 MB"
1073741824.ToFileSizeString("B",1);//returns"1024 MB"
1073741824.ToFileSizeString("GB",1);//returns"1024 MB"
```

# ToOrdinal()
In written languages, an ordinal indicator is a character, or group of characters, following a numeral denoting that it is an ordinal number, rather than a cardinal number.
In English orthography, this corresponds to the suffixes -st, -nd, -rd, -th in written ordinals (represented either on the line 1st, 2nd, 3rd, 4th or as superscript).
#### When to use it?
converts 1 to 1st. Or converts 13 to 13th.
#### Example:
```csharp
0.ToOrdinal();//returns "0th"
1.ToOrdinal();//returns "1st"
2.ToOrdinal();//returns "2nd"
3.ToOrdinal();//returns "3rd"
4.ToOrdinal();//returns "4th"
5.ToOrdinal();//returns "5th"
6.ToOrdinal();//returns "6th"
7.ToOrdinal();//returns "7th"
8.ToOrdinal();//returns "8th"
9.ToOrdinal();//returns "9th"
10.ToOrdinal();//returns "10th"
100.ToOrdinal();//returns "100th"
1000.ToOrdinal();//returns "1000th"

21.ToOrdinal();//returns "21st"
22.ToOrdinal();//returns "22nd"
23.ToOrdinal();//returns "23rd"
24.ToOrdinal();//returns "24th"
20.ToOrdinal();//returns "20th"

```

# ToGuid()
Concerts this integer value to GUID.

#### When to use it?
When you want to convert an `Integer` value to GUID.
#### Example:
```csharp
0.ToGuid();//returns {00000000-0000-0000-0000-000000000000}
1.ToGuid();//returns {00000001-0000-0000-0000-000000000000}
2.ToGuid();//returns {00000002-0000-0000-0000-000000000000}
100.ToGuid();//returns {00000064-0000-0000-0000-000000000000}
1000.ToGuid();//returns {000003e8-0000-0000-0000-000000000000}
10000.ToGuid();//returns {00002710-0000-0000-0000-000000000000}
1234567890.ToGuid();//returns {499602d2-0000-0000-0000-000000000000}
(-12323).ToGuid();//returns {ffffcfdd-0000-0000-0000-000000000000}
```

# Tostring({format})
This method will not change the original value, but it will return a formatted string.

#### When to use it?
When you want to give a special format to a `Integer` object in your applications.

#### Example:

**Thousands separator**

To format `Integer` to string with use of thousands separator use zero and comma separator before an usual float formatting pattern, e.g. pattern "0,0" formats the number to use thousands separators.
```csharp
(12345).ToString("0,0");     //returns "12,345"
```

**Custom formatting for negative numbers and zero**

If you need to use custom format for negative `Integer` numbers or zero, use semicolon separator ";" to split pattern to three sections. The first section formats positive numbers, the second section formats negative numbers and the third section formats zero. If you omit the last section, zero will be formatted using the first section.

```csharp
(123).ToString("0;minus 0;zero" );   //returns "123"
(-123).ToString("0;minus 0;zero");  //returns "minus 123"
(0).ToString("0;minus 0;zero");        //returns "zero"
(23).ToString("000.000;(000);zero");  //returns "023"
(-23).ToString("000.000;(000);zero"); //returns "(023)"
```

**Other Samples**
```csharp
(12).ToString("my number is 0");   //returns "my number is 12"
(12).ToString("0aaa.bbb0");          //returns "1aaa.bbb2"
(1532).ToString("C");  //returns "£1,532.00"
(-1532).ToString("C"); //returns "-£1,532.00"
(1532).ToString("N" );  //returns "1,532"
(1).ToString("P");  //returns "100.00 %"
(2).ToString("P");  //returns "200.00 %"

```

# toWordString()
 Converts an `Integer` values as words/

#### When to use it?
When you want to show an `Integer` values as words in your applications.

#### Example:
```csharp
0.ToWordString();//returns "zero"
1.ToWordString();//returns "one"
10.ToWordString();//returns "one""ten"
100.ToWordString();//returns "one""one hundred "
1000.ToWordString();//returns "one""one thousand "
2000.ToWordString();//returns "one""two thousand "
20000.ToWordString();//returns "one""twenty thousand "
200000.ToWordString();//returns "one""two hundred  thousand "
2000000.ToWordString();//returns "one""two million "
20000000.ToWordString();//returns "one""twenty million "
200000000.ToWordString();//returns "one""two hundred  million "
2000000000.ToWordString();//returns "one""two thousand  million "
1234567890.ToWordString();//returns "one thousand two hundred and thirty-four million five hundred and sixty-seven thousand eight hundred and ninety"
(-12323).ToWordString();//returns "minus twelve thousand three hundred and twenty-three"
```
