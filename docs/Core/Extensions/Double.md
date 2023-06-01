
# Double Extension Methods
>`Double`  extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.
`double` is a 8 byte floating point type that its range is (+/-)5.0 x 10-324  to (+/-)1.7 x 10308. Its default value is 0.0D and its range of value is 15 digits.

## AlmostEquals({otherValue}, {tolerance})
Determines if this double value is almost equal to the specified other value.
This should be used instead of == or != operators due to the nature of double processing in .NET.
{tolerance} specifies the tolerated level of difference. Default value of {tolerance} is 0.00001.
#### When to use it?
When you want to know whether 2 double or decimal values are equal according to the number of digits after dot in your applications.

#### Example:
```csharp
(121.12).AlmostEquals(121.1)       ;//returns false
(121.12).AlmostEquals(121.12)      ;//returns true
(121.12).AlmostEquals(121.1,0.01)  ;//returns false
(121.12).AlmostEquals(121.1,0.1)   ;//returns true
(121.12345).AlmostEquals(121.12345,0.1)          ;//returns true
(121.12345).AlmostEquals(121.12345,0.01)         ;//returns true
(121.12345).AlmostEquals(121.12345,0.001)        ;//returns true
(121.12345).AlmostEquals(121.12345,0.0001)       ;//returns true
(121.12345).AlmostEquals(121.12345,0.00001)      ;//returns true
(121.12345).AlmostEquals(121.12345,0.000001)     ;//returns true
(121.12345).AlmostEquals(121.12345,0.0000001)    ;//returns true
(121.12345).AlmostEquals(121.123456,0.0000001)   ;//returns false
(121.12345).AlmostEquals(121.123456,0.000001)    ;//returns false
(121.12345).AlmostEquals(121.123456,0.00001)     ;//returns true
(121.12345).AlmostEquals(121.123456,0.0001)      ;//returns true
```

## AsPercentageOf({total}, {multiplyBy100}, {roundTo})
Return this value as a percentages the of the given total.
#### When to use it?
When you want to return a double or decimal value as a percentages the of the given total in your applications.

#### Example:
```csharp
(57.23).AsPercentageOf(100,true,0); //returns 57
(57.23).AsPercentageOf(100,true,1); //returns 57.2
(57.23).AsPercentageOf(100,true,2); //returns 57.23
(57.26).AsPercentageOf(100,true,2); //returns 57.26
(57.26).AsPercentageOf(100,true,1); //returns 57.3
(57.26).AsPercentageOf(100,false,1); //returns 0.6
(57.26).AsPercentageOf(100,false,0); //returns 1
(57.26).AsPercentageOf(100,false,2); //returns 0.57
(57.26).AsPercentageOf(100,false,3); //returns 0.573
(57.26).AsPercentageOf(2500,false,3); //returns 0.023
(57.26).AsPercentageOf(2500,true,3); //returns  2.29
(57.26).AsPercentageOf(2500,true,2); //returns  2.29
(57.26).AsPercentageOf(2500,true,1); //returns  2.3
(57.26).AsPercentageOf(2500,true,0); //returns  2
(67.26).AsPercentageOf(2500,true,0); //returns  3
```

## CompareTo({otherValue})
Compare two values and returns 0, 1 or -1.
If this value is equal to {othervalue}, it returns 0.
If this value is greater than {othervalue}, it returns 1.
If this value is smaller than {othervalue}, it returns -1.

#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(120.1).CompareTo(120.1);// returns 0
(120.1).CompareTo(110.1);// returns 1
(120.1).CompareTo(130.1);// returns -1
(120.1).CompareTo(null);// returns 1

Double? x1,x2= null;
(x1).CompareTo(x2); // returns 0
(x1).CompareTo(100);// returns  -1
(100).CompareTo(x2);// returns  1
```
## LimitMax({maxValue})
Determines the maximum limitation of two values. If this value is smaller than {maxValue}, this value is returned, owherwise, {maxvalue} is returned.
#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(121.12345).LimitMax(121); //returns  121
(121.12345).LimitMax(122); //returns  121.12345
```

## LimitMin({minValue})
Determines the minimum limitation of two values. If this value is greater than {minValue}, this value is returned, owherwise, {minvalue} is returned.
#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(121.12345).LimitMin(122); //returns  122
(121.12345).LimitMin(121); //returns  121.12345
```

## LimitWithIn({minValue},{maxValue})
Determines the minimum and maximum limitation of two values. If this value is between {minValue} and {maxValue}, this value is returned.
If this value is smaller than {minvalue}, {minvalue} is returned.
If this value is greater than {maxvalue}, {maxvalue} is returned.
#### When to use it?
When you want to set a limitation on the value in your applications.

#### Example:
```csharp
(122.1).LimitWithin(121.1,123.1);// returns 122.1
(124.1).LimitWithin(121.1,123.1);// returns 123.1
(120.1).LimitWithin(121.1,123.1);// returns 121.1
```

## Round({digits})
In `Olive`, `Round()` is a `Double` and `Decimal` class extension method swhich is used to round a value to the nearest integer or to the particular number of fractional digits({digits}). 
#### When to use it?
When you want to round a Double or Decimal value in your applications.

#### Example:
```csharp
Double x = 10.7;
x.Round(0).ToString;//returns "11"
x = 10.71;
x.Round(1).ToString();//returns "10.7"
x.Round(2).ToString();//returns "10.71"
x.Round(3).ToString();//returns "10.71"
x = 10.75;
x.Round(0).ToString();//returns "11"
x.Round(1).ToString();//returns "10.8"
x.Round(2).ToString();//returns "10.75"
x.Round(3).ToString();//returns "10.75"
x = 10.79;
x.Round(0).ToString();//returns "11"
x.Round(2).ToString();//returns "10.79"
x.Round(1).ToString();//returns "10.8"
x = 10.796;
x.Round(0).ToString();//returns "11"
x.Round(1).ToString();//returns "10.8"
x.Round(2).ToString();//returns "10.8"
x.Round(3).ToString();//returns "10.796"
x.Round(4).ToString();//returns "10.796"
```          
 


## RoundUpToNearest({roundIntervals})
Rounds up to nearest value.
#### When to use it?
When you want to rounds up to nearest value in your applications.

#### Example:
```csharp
(50.00).RoundUpToNearest(1); // returns   50 ( 1 * 50 = 50, so 50 returned)
(50.00).RoundUpToNearest(2); // returns   50 ( 2 * 50 = 50, so 50 returned)
(50.00).RoundUpToNearest(3); // returns   51 ( 3 * 17 = 51, so 51 returned)
(20.00).RoundUpToNearest(3); // returns   21 ( 3 * 7 = 21, so 21 returned)
(52.00).RoundUpToNearest(3); // returns   54 ( 3 * 18 = 54, so 54 returned)
(121.00).RoundUpToNearest(30); // returns 150( 3 * 50 = 150, so 150 returned)
```

## RoundDownToNearest({roundIntervals})
Rounds down to nearest value.
#### When to use it?
When you want to rounds down to nearest value in your applications.

#### Example:
```csharp
(50.00).RoundDownToNearest(1); // returns   50 ( 1 * 50 = 50, so 50 returned)
(50.00).RoundDownToNearest(2); // returns   50 ( 2 * 50 = 50, so 50 returned)
(50.00).RoundDownToNearest(3); // returns   51 ( 3 * 16 = 48, so 48 returned)
(20.00).RoundDownToNearest(3); // returns   21 ( 3 * 6 = 18, so 18 returned)
(52.00).RoundDownToNearest(3); // returns   54 ( 3 * 17 = 51, so 51 returned)
(121.00).RoundDownToNearest(30); // returns 150( 3 * 40 = 120, so 120 returned)
```
## ToInformalMoneyString()
Drops the floating point digits from the end of the money string.
#### When to use it?
When you want to show a Double or Decimal object as a `Currency` in your applications.

#### Example:
```csharp
(1500).ToInformalMoneyString();  //returns "£1500"
(1532.236).ToInformalMoneyString();  //returns "£1,532.24"
(-1532.236).ToInformalMoneyString(); //returns "-£1,532.24"
(0.0).ToInformalMoneyString();  //returns "£0"

(15.1).ToInformalMoneyString(); //returns "$15.10"
(150.1).ToInformalMoneyString(); //returns "$150.10"
(1500.1).ToInformalMoneyString(); //returns "$1,500.10"
(15000.1).ToInformalMoneyString(); //returns "$15,000.10"
(150.0).ToInformalMoneyString(); //returns "$150"
(1500.0).ToInformalMoneyString(); //returns "$150k"
(15000.0).ToInformalMoneyString(); //returns "$1,500k"
(150000.0).ToInformalMoneyString(); //returns "$15,000k"
(1500000.0).ToInformalMoneyString(); //returns "$150m"
(15000000.0).ToInformalMoneyString(); //returns "$1,500m"
(150000000.0).ToInformalMoneyString(); //returns "$15,000m"
(1500000000.0).ToInformalMoneyString(); //returns "$150bn"
(150000000.1).ToInformalMoneyString(); //returns "$150,000,000.10"
```
## ToRadians()
Convert an angle in Degrees to Radians.
#### When to use it?
When you want to find the distance between a two co-ordinates in your applications. To do this you need to convert the lat/lng co-ordinates into radians.

#### Example:
```csharp
(23.0).ToRadians();// returns 0.40142572795869574
```
## Tostring({format})
This method will not change the original value, but it will return a formatted string.
#### When to use it?
When you want to give a special format to a `Double` or `Decimal` object in your applications.

#### Example:
- Digits after `decimal` point
This example formats double to string with fixed number of decimal places. For two decimal places use pattern "00". 
If a double number has less decimal places, the rest digits on the right will be zeroes. 
If it has more decimal places, the number will be rounded.
```csharp
(123.4567).ToString("00");   //returns "123.46"
(123.4).ToString("00");         //returns "123.40"
(123.0).ToString("00");         //returns "123.00"
```
Next example formats double to string with floating number of decimal places. E.g. for maximal two decimal places use pattern "0.##".
```csharp
(123.4567).ToString("0.##", 123.4567);   //returns "123.46"
(123.4).ToString("0.##", 123.4);         //returns "123.4"
(123.0).ToString("0.##", 123.0);         //returns "123"
```
- Digits before decimal point
If you want a float number to have any minimal number of digits before decimal point use N-times zero before decimal point. E.g. pattern "00.0" formats a float number to string with at least two digits before decimal point and one digit after that.
```csharp
// at least two digits before decimal point
(123.4567).ToString("00.0");      //returns "123.5"
(23.4567).ToString("00.0");       //returns "23.5"
(3.4567).ToString("00.0");        //returns "03.5"
(-3.4567).ToString("00.0");       //returns "-03.5"
```
- Thousands separator
To format double to string with use of thousands separator use zero and comma separator before an usual float formatting pattern, e.g. pattern "0,0.0" formats the number to use thousands separators and to have one decimal place.
```csharp
(12345.67).ToString("0,0.0");     //returns "12,345.7"
(12345.67).ToString.("0,0");       //returns "12,346"
```
- Custom formatting for negative numbers and zero
If you need to use custom format for negative float numbers or zero, use semicolon separator ";" to split pattern to three sections. 
The first section formats positive numbers, the second section formats negative numbers and the third section formats zero.
If you omit the last section, zero will be formatted using the first section.
```csharp
(123.4567).ToString("0.00;minus 0.00;zero" );   //returns "123.46"
(-123.4567).ToString("0.00;minus 0.00;zero");  //returns "minus 123.46"
(0.0).ToString("0.00;minus 0.00;zero");        //returns "zero"
(23.43).ToString("000.000;(000.000);zero");  //returns "023.430"
(-23.43).ToString("000.000;(000.000);zero"); //returns "(023.430)"
```
- Setting a fixed amount of digits before the decimal point
To set a minimum amount of three digits before the decimal point use the format string "000.##".
```csharp
(1.2345).Tostring("00.000}");    //returns "01.235"
(1.2345).Tostring("000.000}");   //returns "012.345"
(123.456).Tostring("0000.000}"); //returns "0123.456"
```
- Other Samples
```csharp
(12.3).ToString("my number is 0.0", 12.3);   //returns "my number is 12.3"
(12.3).ToString("0aaa.bbb0", 12.3);          //returns "12aaa.bbb3"
(1532.236).ToString("C");  //returns "£1,532.24"
(-1532.236).ToString("C"); //returns "-£1,532.24"
(1532.236).ToString("E");  //returns "1.532236E+003"
(-1532.236).ToString("E"); //returns "-1.532236E+003"
(1532.236).ToString("G" );  //returns "1532.236"
(-1532.236).ToString("G" ); //returns "-1532.236"
(1532.236).ToString("N" );  //returns "1,532.24"
(-1532.236).ToString("N"); //returns "-1,532.24"
(0.1532).ToString("P");    //returns "15.32 %"
(-0.1532).ToString("P");   //returns "-15.32 %"
```


## Truncate({places})
In mathematics and computer science, truncation is the term for limiting the number of digits right of the decimal point, by discarding the least significant ones.
> Note In some cases, truncating would yield the same result as rounding, but truncation does not round up or round down the digits; it merely cuts off at the specified digit.
#### When to use it?
When you want to round a `Double` or `Decimal` value in your applications.

#### Example:
```csharp

Double x = 10;
x.Truncate(0);//returns 10
x.Truncate(1);//returns 10
x.Truncate(2);//returns 10
x = 10.1;
x.Truncate(0);//returns 10
x.Truncate(1);//returns 10.1
x.Truncate(2);//returns 10.1
x = 10.9;
x.Truncate(0);//returns 10
x.Truncate(1);//returns 10.9
x.Truncate(2);//returns 10.9
x = 10.19;
x.Truncate(0);//returns 10
x.Truncate(1);//returns 10.1
x.Truncate(2);//returns 10.19
x.Truncate(3);//returns 10.19
x = 10.195;
x.Truncate(0);//returns 10
x.Truncate(1);//returns 10.1
x.Truncate(2);//returns 10.19
x.Truncate(3);//returns 10.195
``` 
