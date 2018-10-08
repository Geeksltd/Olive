

# DateTime Extension Methods
>Date And Time extension methods are really useful in all frameworks. Olive has various methods which you can use in your applications.
The year that you can use is between 1 and 9999.
## IsEnglishHoliday()
Determines if a specified date is an English national holiday or weekend.
>Note: 
This method returns true if the date is one of these days:
- Weekends (Saturdays and Sundays)
- 1 January(New Year's Day)
- 1st Monday in May(Early May Bank Holiday)
- Last Monday in May(Spring Bank Holiday)
- Last Monday in August(Late Summer Bank Holiday)
- December 25 Christmas Day
- December 26 Boxing Day
- Easter Mondays
- Good Fridays

#### When to use it?
When you want to know a specific date that selected by user is an English national holiday or weekend in your applications.

#### Example:
```csharp
var dateVariable1= new DateTime(2018, 10, 5);
dateVariable1.IsEnglishHoliday(); // returns False because it is Friday

var dateVariable2 = new DateTime(2018, 10, 6);
dateVariable2.IsEnglishHoliday(); // returns True because it is Saturday

var dateVariable3 = new DateTime(2018, 10, 7);
dateVariable3.IsEnglishHoliday(); // returns True because it is Sunday

var dateVariable4 = new DateTime(2019, 04, 22);
dateVariable4.IsEnglishHoliday(); // returns True because it is a Easter Monday

var dateVariable5 = new DateTime(2019, 08, 26);
dateVariable5.IsEnglishHoliday(); // returns True because it is Last Monday in August

```

## GetUpcoming({DayOfWeek},{skipTodayBoolean})
Gets the first upcoming specified week day.
>Note: If skipTodayBoolean is true, method starts from tomorrow.
#### When to use it?
When you want to know what is the next specified weekend day in your applications.
#### Example:
```csharp

var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.GetUpcoming(DayOfWeek.Monday); // returns the next Monday date          
dateVariable1.GetUpcoming(DayOfWeek.Monday,true); // returns the next Monday date, if today is Monday, it returns the Monday date of next week.   

```


## GetLast({DayOfWeek},{skipTodayBoolean})
Gets the last occurance of the specified week day.
>Note: If skipTodayBoolean is true, method starts from yesterday.
#### When to use it?
When you want to know what is the previous specified weekend day in your applications.
#### Example:
```csharp

var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.GetLast(DayOfWeek.Monday); // returns the previous Monday date          
dateVariable1.GetLast(DayOfWeek.Monday,true); // returns the previous Monday date, if today is Monday, it returns the Monday date of previous week.        

```

## IsOlderThan({TimeSpan})
- It returns True if the date is older than {TimeSpan}.
- It returns false if the date is not older than {TimeSpan}.
#### When to use it?
When you want to know whether the date is older than another date or not.
#### Example:
```csharp
var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.IsOlderThan(TimeSpan.FromDays(1)); // returns False because tomorrow is not older than today.
dateVariable1.IsOlderThan(TimeSpan.FromDays(-1)); // returns True because yesterday is older than today.

```

## IsNewerThan({TimeSpan})
- It returns True if the date is newer than {TimeSpan}.
- It returns false if the date is not newer than {TimeSpan}.
#### When to use it?
When you want to know whether the date is newer than another date or not.
It returns True or false.
#### Example:
```csharp
var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.IsNewerThan(TimeSpan.FromDays(1)); // returns True because tomorrow is newer than today.
dateVariable1.IsNewerThan(TimeSpan.FromDays(-1)); // returns False because yesterday is not newer than today.

```

## IsAfterOrEqualTo({DateTime})
- It returns True if the date is bigger or equal {DateTime}.
- It returns False if the date is before {DateTime}.
#### When to use it?
When you want to compare two date value in your application.
It returns True or false.
#### Example:
```csharp

var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.IsAfterOrEqualTo(new DateTime(2010, 10 , 10)); // returns True
```

## IsBeforeOrEqualTo({DateTime})
- It returns True if the date is smaller or equal to {DateTime}.
- It returns False if the date is after {DateTime}.
#### When to use it?
When you want to compare two date value in your application.
It returns True or false.
#### Example:
```csharp

var dateVariable1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
dateVariable1.IsBeforeOrEqualTo(new DateTime(2010, 10 , 10)); // returns false
```

## IsInSameWeek({otherDateTime})
Determines whether this day is in the same week (Monday to Sunday) as the specified other date.
#### When to use it?
When you want to compare two date value in your application and know they are in the same week or not.
It returns True or false.
#### Example:
```csharp

var dateVariable1 = new DateTime(2018, 10, 8); //Monday
var dateVariable2 = new DateTime(2018, 10, 9); // Tuesday
dateVariable1.IsInSameWeek(dateVariable2); // returns true
```

## IsInSameMonth({otherDateTime})
Determines whether this day is in the same month as the specified other date.
#### When to use it?
When you want to compare two date value in your application and know they are in the same month or not.
It returns True or false.
#### Example:
```csharp

var dateVariable1 = new DateTime(2018, 10, 8); 
var dateVariable2 = new DateTime(2018, 10, 9);
dateVariable1.IsInSameMonth(dateVariable2); // returns true
```

## DaysInYear()
Gets the number of days in this year.
Every 4 years, a year comes along that has one more day to it, meaning 366 days in total. Those years with 366 days are leap years. More specifically, the year in which the month of February has 29 days (instead of the usual 28 days) is deemed a leap year. For example, the year 2020 was a leap year.
It return 365 if the year is not a leap year.
It return 366 if the year is a leap year.
#### When to use it?
When you want to know how many days are in the year of the date in your application.
In some applications such as Payroll system, you need the number of day of the year.
#### Example:
```csharp

var dateVariable1 = new DateTime(2018, 10, 8); 
dateVariable1.DaysInYear(); // returns 365

var dateVariable2 = new DateTime(2020, 10, 8); 
dateVariable2.DaysInYear(); // returns 366 

```

## DaysInMonth()
Gets the number of days in this month.
The number of days in month for the specified year.For example, if month equals 2 for February, the return value is 28 or 29 depending upon whether year is a
leap year.
>Note: It returns a number ranging from 28 to 31.
- January - 31 days
- February - 28 days 
- March - 31 days
- April - 30 days
- May - 31 days
- June - 30 days
- July - 31 days
- August - 31 days
- September - 31 days
- October - 31 days
- November - 30 days
- December - 31 days

#### When to use it?
When you want to know how many days are in the month of the date in your application.
In some applications such as Payroll system, you need the number of day of the month.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 8); 
dateVariable1.DaysInMonth(); // returns 31

var dateVariable2 = new DateTime(2019, 2, 1); 
dateVariable2.DaysInMonth(); // returns 28 

var dateVariable2 = new DateTime(2020, 2, 1); 
dateVariable2.DaysInMonth(); // returns 29 because 2020 is a leap year.

```

## GetBeginningOfWeek({day})
Gets the mid-night of Monday of this week.
- It returns DateTime
#### When to use it?
When you want to know the first day of the week of the date in your application.
In some applications such as Payroll or Sales systems, you need the first day of the week of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetBeginningOfWeek(); // returns 08/10/2018
```


## GetEndOfWeek({day})
Gets one tick before the start of next week.
- It returns DateTime
#### When to use it?
When you want to know the last day of the week of the date in your application.
In some applications such as Payroll or Sales systems, you need the last day of the week of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetEndOfWeek(); // returns 14/10/2018

```


