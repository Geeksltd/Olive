



# DateTime Extension Methods
>`Date` And `Time` extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.
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
When you want to know a specific `date` that selected by user is an English national holiday or weekend in your applications.

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
>Note: If `skipTodayBoolean` is `true`, method starts from tomorrow.
#### When to use it?
When you want to know what is the next specified weekend day in your applications.
#### Example:
```csharp

var dateVariable1 = DateTime.Today;
dateVariable1.GetUpcoming(DayOfWeek.Monday); // returns the next Monday date          
dateVariable1.GetUpcoming(DayOfWeek.Monday,true); // returns the next Monday date, if today is Monday, it returns the Monday date of next week.   

```

## GetLast({DayOfWeek},{skipTodayBoolean})
Gets the last occurance of the specified week day.
>Note: If skipTodayBoolean is `true`, method starts from yesterday.
#### When to use it?
When you want to know what is the previous specified weekend day in your applications.
#### Example:
```csharp

var dateVariable1 = DateTime.Today;
dateVariable1.GetLast(DayOfWeek.Monday); // returns the previous Monday date          
dateVariable1.GetLast(DayOfWeek.Monday,true); // returns the previous Monday date, if today is Monday, it returns the Monday date of previous week.        

```

## IsOlderThan({TimeSpan})
- It returns `True` if the date is older than {TimeSpan}.
- It returns `false` if the date is not older than {TimeSpan}.
#### When to use it?
When you want to know whether the date is older than another date or not.
#### Example:
```csharp
var dateVariable1 = DateTime.Today;
dateVariable1.IsOlderThan(TimeSpan.FromDays(1)); // returns False because tomorrow is not older than today.
dateVariable1.IsOlderThan(TimeSpan.FromDays(-1)); // returns True because yesterday is older than today.

```

## IsNewerThan({TimeSpan})
- It returns `True` if the date is newer than {TimeSpan}.
- It returns `false` if the date is not newer than {TimeSpan}.
#### When to use it?
When you want to know whether the date is newer than another date or not.
It returns `True` or `false`.
#### Example:
```csharp
var dateVariable1 = DateTime.Today;
dateVariable1.IsNewerThan(TimeSpan.FromDays(1)); // returns True because tomorrow is newer than today.
dateVariable1.IsNewerThan(TimeSpan.FromDays(-1)); // returns False because yesterday is not newer than today.

```

## IsAfterOrEqualTo({DateTime})
- It returns `True` if the date is bigger or equal {DateTime}.
- It returns `False` if the date is before {DateTime}.
#### When to use it?
When you want to compare two date value in your application.
It returns `True` or `false`.
#### Example:
```csharp

var dateVariable1 = DateTime.Today;
dateVariable1.IsAfterOrEqualTo(new DateTime(2010, 10 , 10)); // returns True
```

## IsBeforeOrEqualTo({DateTime})
- It returns `True` if the date is smaller or equal to {DateTime}.
- It returns `False` if the date is after {DateTime}.
#### When to use it?
When you want to compare two date value in your application.
It returns `True` or `false`.
#### Example:
```csharp

var dateVariable1 = DateTime.Today;
dateVariable1.IsBeforeOrEqualTo(new DateTime(2010, 10 , 10)); // returns false
```

## IsInSameWeek({otherDateTime})
Determines whether this day is in the same week (Monday to Sunday) as the specified other date.
#### When to use it?
When you want to compare two date value in your application and know they are in the same week or not.
It returns `True` or `false`.
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
It returns `True` or `false`.
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
The number of days in month for the specified year.For example, if month equals 2 for February, the return value is 28 or 29 depending upon whether year is a leap year.
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
In some applications such as `Payroll` system, you need the number of day of the month.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 8); 
dateVariable1.DaysInMonth(); // returns 31

var dateVariable2 = new DateTime(2019, 2, 1); 
dateVariable2.DaysInMonth(); // returns 28 

var dateVariable2 = new DateTime(2020, 2, 1); 
dateVariable2.DaysInMonth(); // returns 29 because 2020 is a leap year.

```

## GetBeginningOfWeek()
Gets the mid-night of Monday of this week.
- It returns `DateTime`.
#### When to use it?
When you want to know the first day of the week of the date in your application.
In some applications such as `Payroll` or `Sales` systems, you need the first day of the week of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetBeginningOfWeek(); // returns "10/10/2018 12:00:00 AM"
```


## GetEndOfWeek()
Gets one tick before the start of next week.
- It returns `DateTime`.
#### When to use it?
When you want to know the last day of the week of the date in your application.
In some applications such as `Payroll` or `Sales` systems, you need the last day of the week of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetEndOfWeek(); // returns "10/14/2018 11:59:59 PM"

```

## GetBeginningOfMonth()
Gets the mid-night of the first day of this month.
- It returns `DateTime`.
#### When to use it?
When you want to know the first day of the month of date in your application.
In some applications such as `Payroll` or `Sales` systems, you need the first day of the month of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetBeginningOfMonth(); // returns "10/01/2018 12:00:00 AM"

```

## EndOfDay()
Gets the end of this day (one tick before the next day).
- It returns `DateTime` or `null`.
#### When to use it?
When you want to know the last tick of the date in your application.
In some applications such as `Payroll` or `Sales` systems, you need the last tick of the date.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.EndOfDay(); // returns "10/10/2018 11:59:59 PM"

DateTime? dateVariable2 = null; 
dateVariable2.EndOfDay(); // returns ""

```

 

## IsInTheFuture()
Determines whether this date is in the future.
- It returns `true` if the date after or equal to `LocalTime.Today`.
- It returns `false` if the date is today.
#### When to use it?
When you want to know a DateTime object is greater than today or not in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 5, 5); 
dateVariable1.IsInTheFuture(); // returns false

var dateVariable2 = DateTime.Today; 
dateVariable2.IsInTheFuture(); // returns false

```

## IsToday()
Determines whether this date is in the future.
- It returns `true` if the the date is equal to `LocalTime.Today`.
#### When to use it?
When you want to know a DateTime object is today in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 5, 5); 
dateVariable1.IsToday(); // returns false

var dateVariable2 = DateTime.Today; 
dateVariable2.IsToday(); // returns true

```


## IsInThePast()
Determines whether this date is in the past.
- It returns `true` if the the date is smaller than `LocalTime.Today`.
#### When to use it?
When you want to know a `DateTime` object is smaller than in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 5, 5); 
dateVariable1.IsInThePast(); // returns true

var dateVariable2 = DateTime.Today; 
dateVariable2.IsToday(); // returns false

```

## IsAfter({day})
Determines whether this date is greater than {day}.
- It returns `true` if this date is greater than {day}.
#### When to use it?
When you want to know a `DateTime` object is greater than another `Datetime` value in your application.
#### Example:

```csharp

var dateVariable1 = DateTime.Today.AddDays(-1);
var dateVariable2 = DateTime.Today;
dateVariable2.IsAfter(dateVariable1); // returns true

```

## IsBefore({day})
Determines whether this date is smaller than {day}.
- It returns `true` if this date is smaller than {day}.
#### When to use it?
When you want to know a DateTime object is smaller than another `Datetime` value in your application.
#### Example:

```csharp

var dateVariable1 = DateTime.Today.AddDays(-1);
var dateVariable2 = DateTime.Today;
dateVariable2.IsBefore(dateVariable1); // returns false

```

## ToSmallTime()
Determines whether this date is smaller than {day}.
- It returns `true` if this date is smaller than {day}.
#### When to use it?
When you want to know a DateTime object is smaller than another `Datetime` value in your application.
#### Example:

```csharp

var dateVariable1 = DateTime.Now;
dateVariable2.ToSmallTime(); // returns e.g: 2:15pm or 1:43am

```

## IsWeekend()
Determines whether this date is smaller than {day}.
- It returns `true` if this date is smaller than {day}.
#### When to use it?
When you want to know a DateTime object is smaller than another `Datetime` value in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 7);
dateVariable2.IsWeekend(); // returns true because this day is Sunday

var dateVariable2 = new DateTime(2018, 10, 8);
dateVariable2.IsWeekend(); // returns false because this day is Monday

```

## NextWorkingDay()
Gets the next working day.
#### When to use it?
When you want to know the next working day in your application. You can add a specific working day after today.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 7); // Monday
dateVariable2.NextWorkingDay(); // returns "08/10/2018 12:00:00 AM" Tuesday

var dateVariable2 = new DateTime(2018, 10, 11); // Thursday
dateVariable2.NextWorkingDay(); // returns "15/10/2018 12:00:00 AM" next Monday

```

## PreviousWorkingDay()
Gets the previous working day.
#### When to use it?
When you want to know the next working day in your application. You can add a specific working day after today.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 8); // Monday
dateVariable2.PreviousWorkingDay(); // returns "05/10/2018 12:00:00 AM" Friday

var dateVariable2 = new DateTime(2018, 10, 9); // Teusday
dateVariable2.PreviousWorkingDay(); // returns "08/10/2018 12:00:00 AM" next Monday

```

 
 ## GetDaysInBetween({date},{inclusive})
Gets the days between this day and the specified other day.
- if {inclusive} is `true`, result has the first and last days.
#### When to use it?
When you want to know the next working day in your application. You can add a specific working day after today.
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 08);
dateVariable1.GetDaysInBetween(new DateTime(2018,10,12),**true**).ToLinesString(); 
//returns "10/08/2018 12:00:00 AM" "10/09/2018 12:00:00 AM" "10/10/2018 12:00:00 AM" "10/11/2018 12:00:00 AM" "10/12/2018 12:00:00 AM"

dateVariable1.GetDaysInBetween(new DateTime(2018,10,12)).ToLinesString(); 
//returns "10/09/2018 12:00:00 AM" "10/10/2018 12:00:00 AM" "10/11/2018 12:00:00 AM" 

dateVariable1.GetDaysInBetween(new DateTime(2018,10,08)).ToLinesString(); //returns "" 

dateVariable1.GetDaysInBetween(new DateTime(2018,10,08),true).ToLinesString(); //returns "10/08/2018 12:00:00 AM" 

```

 ## AddWorkingDays({dayNumber})
Gets the day after {dayNumber} working day.
#### When to use it?
When you want to know the day after {dayNumber} working day in your application. 
- You can use minus number in {daynumber}.
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 8); // Monday
dateVariable1.AddWorkingDays(0); // returns "08/10/2018 12:00:00 AM" Monday

dateVariable1.AddWorkingDays(1); // returns "09/10/2018 12:00:00 AM" Tuesday

dateVariable1.AddWorkingDays(-1); // returns "05/10/2018 12:00:00 AM" previous Thursday

```
