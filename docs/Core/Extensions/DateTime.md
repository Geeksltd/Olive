# DateTime Extension Methods
>`Date` And `Time` extension methods are really useful in all frameworks. `Olive` has various methods which you can use in your applications.
The year that you can use is between 1 and 9999.

## AddWeeks({numberofWeeks})
Adds the specified number of weeks and returns the result.
{numberofWeeks} can be minus for subtracting specific number of weeks from the date.
#### When to use it?
When you want to know after or before specific weeks to a date, what is result.

#### Example:
```csharp
new DateTime(2018,10,12).AddWeeks(1).ToString();  //returns "10/19/2018 12:00:00 AM"
new DateTime(2018,10,12).AddWeeks(-1).ToString(); //returns "10/05/2018 12:00:00 AM"
new DateTime(2018,10,12).AddWeeks(-2).ToString(); //returns "9/28/2018 12:00:00 AM"
new DateTime(2018,10,12).AddWeeks(2).ToString();  //returns "10/26/2018 12:00:00 AM"
new DateTime(1,1,1).AddWeeks(2).ToString();       //returns "1/15/0001 12:00:00 AM"
new DateTime(9999,5,12).AddWeeks(35).ToString(); // throws Exception thrown: 'System.ArgumentOutOfRangeException' in mscorlib.dll
new DateTime(1,1,1).AddWeeks(-2).ToString()' // throws  Exception thrown: 'System.ArgumentOutOfRangeException' in mscorlib.dll
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

dateVariable1.AddWorkingDays(-1); // returns "05/10/2018 12:00:00 AM" previous Friday
```

 ## CalculateTotalWorkingHours({period}, {workingStartTime},{workingEndTime},{considerEnglishBankHolidays})
Calculates the total working times in the specified duration which are between the two specified day-hours.
- {period} should be a positive time span.
- {workingStartTime} should be greater than or equal to 0, and smaller than 24.
- {workingEndTime} should be greater than 0, and smaller than or equal to 24.
- If it is not important that whether the day is BankHolidays or not, you should set the considerEnglishBankHolidays on false. The default value is true.
#### When to use it?
This can be used to calculate working hours in a particular duration.
#### Example:

```csharp

new DateTime(2018,10,09).CalculateTotalWorkingHours(TimeSpan.FromDays(1), TimeSpan.FromHours(8), TimeSpan.FromHours(16),false).ToString(); // returns "08:00:00"
new DateTime(2018,10,09).CalculateTotalWorkingHours(TimeSpan.FromDays(2), TimeSpan.FromHours(8), TimeSpan.FromHours(16),false).ToString(); // returns "16:00:00"
```     
If it is not important that whether the day is BankHolidays or not, you should set the considerEnglishBankHolidays on false:

```csharp
new DateTime(2016,1,1).CalculateTotalWorkingHours(TimeSpan.FromDays(1), TimeSpan.FromHours(8), TimeSpan.FromHours(16),false).ToString(); // returns "08:00:00" , this day is BankHolidays, but we set considerEnglishBankHolidays on `false`, so the output is "08:00:00"

new DateTime(2016,1,1).CalculateTotalWorkingHours(TimeSpan.FromDays(1), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "00:00:00" , this day is BankHolidays and we set considerEnglishBankHolidays on `true`, so the output is "00:00:00"
```     

When the period of time consist of a few days, this function control the Bankholiday of each day and calculate all working hours:

```csharp
// calculating from 8 o'clock to 16 o'clock during 1 day to 9 days which set in FromDays parameter.
new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(1), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "08:00:00" , this day is not BankHolidays(Thursday), so the output is "08:00:00".  

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(2), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "08:00:00" , the second day(1/1/2016) is BankHolidays, so the output is only "08:00:00".

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(3), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "08:00:00" , the second day(1/1/2016) is BankHolidays and third day is weekend (2/1/2016,Saturday), so the output is only "08:00:00".

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(4), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "08:00:00" , the second day(1/1/2016) is BankHolidays, third day is weekend (2/1/2016,Saturday) and 4th day is weekend (3/1/2016,Sunday), so the output is only "08:00:00".

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(5), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "16:00:00" , the second day(1/1/2016) is BankHolidays, third day is weekend (2/1/2016,Saturday), 4th day is weekend (3/1/2016,Sunday) and 5th day is not weekand and holiday, so the output is "16:00:00".

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(6), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "1:00:00:00" , 3*8 = 1 day or 24 hours

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(7), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "1:08:00:00"  or 32 hours

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(8), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "1:16:00:00"  or 40 hours

new DateTime(2015,12,31).CalculateTotalWorkingHours(TimeSpan.FromDays(9), TimeSpan.FromHours(8), TimeSpan.FromHours(16)).ToString(); // returns "2:00:00:00"  or 48 hours
```     

## CompareTo({secondDate})
Compare this value with another date. 
- If this value is greater than {secondDate}, it returns "1", otherwise, it returns "-1".
#### When to use it?
When you want to compare two date values in your applications.

#### Example:

```csharp
new DateTime(2018,10,12).CompareTo(new DateTime(2018,10,11)).ToString(); //returns "1"
new DateTime(2018,10,12).CompareTo(new DateTime(2018,10,10)).ToString(); //returns "1"
new DateTime(2018,10,12).CompareTo(new DateTime(2018,10,14)).ToString(); //returns "-1"
new DateTime(2018,10,12).CompareTo(new DateTime(2018,10,15)).ToString()' //returns "-1"

DateTime? dateVariable3 = new DateTime();
dateVariable3.CompareTo(new DateTime(2018,10,15)).ToString(); //returns "-1"
new DateTime(2018,10,12).CompareTo(dateVariable3).ToString(); //returns "1"
```  
## DaysInMonth()
Gets the number of days in this month.
The number of days in month for the specified year.For example, if month equals 2 for February, the return value is 28 or 29 depending upon whether year is a leap year.
> Note: It returns a number ranging from 28 to 31.
- January - 31 days
- February - 28 days / 29 days in leap year.
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

 ## GetBeginningOfQuarter()
 
Returns the Date of the beginning of Quarter for this DateTime value (time will be 00:00:00).
#### When to use it?
When you want to know the Date of the beginning of Quarter for this DateTime value. 
For example, in Sales management system, you should have the first date of each quarter to calclulate amount of sale of that quarter, and report it.
- First quarter, Q1: 1 January – 31 March (90 days or 91 days in leap years)
- Second quarter, Q2: 1 April – 30 June (91 days)
- Third quarter, Q3: 1 July – 30 September (92 days)
- Fourth quarter, Q4: 1 October – 31 December (92 days)
#### Example:

```csharp
new DateTime(2018,10,09).GetBeginningOfQuarter().ToString(); // returns "1/10/2018"
new DateTime(1978,06,10).GetBeginningOfQuarter().ToString(); // returns "1/04/1978"
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
dateVariable1.GetBeginningOfWeek(); // returns "10/08/2018 12:00:00 AM"
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
 ## GetEndOfMonth()
Returns the Date of the end of Month for this DateTime value (time will be 11:59:59).
#### When to use it?
When you want to know the Date of the end of Month for this DateTime value. 
For example, in Payroll system, you should have the last date of each Month to calclulate amount of the wage of that month.

The last day of each month:
- January - 31 
- February - 28 / 29 in leap year.
- March - 31 
- April - 30 
- May - 31 
- June - 30
- July - 31
- August - 31
- September - 31
- October - 31
- November - 30
- December - 31

#### Example:

```csharp
new DateTime(2018,10,09).GetEndOfMonth().ToString(); // returns "31/10/2018"
new DateTime(2020,02,10).GetEndOfMonth().ToString(); // returns "29/02/2020" , 2020 is leap year.
```     
## GetEndOfQuarter()
Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
#### When to use it?
When you want to know the Date of the end of Quarter for this DateTime value. 
For example, in Sales management system, you should have the last date of each quarter to calclulate amount of sale of that quarter, and report it.
First quarter, Q1: 1 January – 31 March (90 days or 91 days in leap years)
Second quarter, Q2: 1 April – 30 June (91 days)
Third quarter, Q3: 1 July – 30 September (92 days)
Fourth quarter, Q4: 1 October – 31 December (92 days)

#### Example:

```csharp
new DateTime(2018,10,09).GetBeginningOfQuarter().ToString(); // returns "31/12/2018"
new DateTime(1978,06,10).GetBeginningOfQuarter().ToString(); // returns "30/06/1978"
```     
## GetEndOfWeek({startofweek})
Gets one tick before the start of next week.
- It returns `DateTime`.
- You can set the first day of week by setting {startofweek}. Default value is Monday.
#### When to use it?
When you want to know the last day of the week of the date in your application.
In some applications such as `Payroll` or `Sales` systems, you need the last day of the week of the date.
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 10); 
dateVariable1.GetEndOfWeek(); // returns "10/14/2018 11:59:59 PM"

dateVariable1.GetEndOfWeek(DayOfWeek.Monday); // returns "10/14/2018 11:59:59 PM"
dateVariable1.GetEndOfWeek(DayOfWeek.Saturday); // returns "10/12/2018 11:59:59 PM"
```
## GetEndOfYear()
Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
#### When to use it?
When you want to know the last day of a specific year in your application. it is useful when you want to provide a specific report ending of a specific year.

#### Example:
```csharp
new DateTime(2018,10,12).GetEndOfYear().ToString(); //returns "12/31/2018 11:59:59 PM"
new DateTime(2020,10,12).GetEndOfYear().ToString(); //retuens "12/31/2020 11:59:59 PM"
```         


## GetLast({CalendarMonth},{day})
Gets the last date with the specified month and day.
#### When to use it?
When you want to know the last day of a specific month and day that ocured before the date in your application. 

#### Example:

```csharp
 new DateTime(2018,10,12).GetLast(CalendarMonth.August,2).ToString(); // returns "8/02/2018 12:00:00 AM"
 new DateTime(2018,10,12).GetLast(CalendarMonth.December,2).ToString(); // returns "12/02/2017 12:00:00 AM"
 new DateTime(2017,10,12).GetLast(CalendarMonth.August,2).ToString();  //returns "8/02/2017 12:00:00 AM"
 new DateTime(2016,10,12).GetLast(CalendarMonth.August,2).ToString(); //returns "8/02/2016 12:00:00 AM"
 new DateTime(2016,10,12).GetLast(CalendarMonth.December,2).ToString(); // returns "12/02/2015 12:00:00 AM"
 new DateTime(2020,10,12).GetLast(CalendarMonth.August,2).ToString(); // returns "8/02/2020 12:00:00 AM"
 new DateTime(2020,10,12).GetLast(CalendarMonth.December,2).ToString(); //returns "12/02/2019 12:00:00 AM"
```          
   
## GetLast({DayOfWeek}, {timeOfDay})
Gets the latest date with the specified day of week and time that is before (or same as) this date.
{DayOfWeek} can be one of these values:
- `Friday` 5	Indicates Friday.
- `Monday`	1	Indicates Monday.
- `Saturday`	6	Indicates Saturday.
- `Sunday`	0	Indicates Sunday.
- `Thursday`	4	Indicates Thursday.
- `Tuesday`	2	Indicates Tuesday.
- `Wednesday`	3	Indicates Wednesday.

#### When to use it?
When you want to know the latest date with the specified day of week and time that is before (or same as) this date in your application.

#### Example:

```csharp
new DateTime(2018,10,12).GetLast(DayOfWeek.Sunday,TimeSpan.FromHours(17)).ToString(); // returns  "10/07/2018 5:00:00 PM"
new DateTime(2018,10,12).GetLast(DayOfWeek.Sunday,TimeSpan.FromHours(7)).ToString(); // returns "10/07/2018 7:00:00 AM"
```          
## GetLast({DayOfWeek},{skipTodayBoolean})
Gets the last occurance of the specified week day.
>Note: If skipTodayBoolean is `true`, method starts from yesterday.
> The default value of `skipTodayBoolean` is `false`.
> {DayOfWeek} can be one of these values:
- `Friday` 5	Indicates Friday.
- `Monday`	1	Indicates Monday.
- `Saturday`	6	Indicates Saturday.
- `Sunday`	0	Indicates Sunday.
- `Thursday`	4	Indicates Thursday.
- `Tuesday`	2	Indicates Tuesday.
- `Wednesday`	3	Indicates Wednesday.
#### When to use it?
When you want to know what is the previous specified weekend day in your applications.
#### Example:
```csharp
var dateVariable1 = DateTime.Today;
dateVariable1.GetLast(DayOfWeek.Monday); // returns the previous Monday date          
dateVariable1.GetLast(DayOfWeek.Monday,true); // returns the previous Monday date, if today is Monday, it returns the Monday date of previous week.        
```
## GetNext({CalendarMonth},{day})
Gets the next date with the specified month and day.
{CalendarMonth} is :
- January = 1
- February = 2
- March = 3
- April = 4
- May = 5
- June = 6
- July = 7
- August = 8
- September = 9
- October = 10
- November = 11
- December = 12

#### When to use it?
When you want to know the next day of a specific month and day that ocured after the date in your application. 

#### Example:

```csharp

new DateTime(2018,10,12).GetNext(CalendarMonth.October,2).ToString(); //returns "10/02/2019 12:00:00 AM"
new DateTime(2020,10,12).GetNext(CalendarMonth.August,2).ToString();  //returns "8/02/2021 12:00:00 AM"
new DateTime(2016,10,12).GetNext(CalendarMonth.December,2).ToString();//returns "12/02/2016 12:00:00 AM"
new DateTime(2016,10,12).GetNext(CalendarMonth.August,2).ToString();  //returns "8/02/2017 12:00:00 AM"
new DateTime(2017,10,12).GetNext(CalendarMonth.August,2).ToString();  //returns "8/02/2018 12:00:00 AM"
new DateTime(2018,10,12).GetNext(CalendarMonth.December,2).ToString();//returns "12/02/2018 12:00:00 AM"
```          
## GetUpcoming({DayOfWeek},{skipTodayBoolean})
Gets the first upcoming specified week day.
> Note: If `skipTodayBoolean` is `true`, method starts from tomorrow.
> The default value of `skipTodayBoolean` is `false`.
> {DayOfWeek} can be one of these values:
- `Friday` 5	Indicates Friday.
- `Monday`	1	Indicates Monday.
- `Saturday`	6	Indicates Saturday.
- `Sunday`	0	Indicates Sunday.
- `Thursday`	4	Indicates Thursday.
- `Tuesday`	2	Indicates Tuesday.
- `Wednesday`	3	Indicates Wednesday.
- 
#### When to use it?
When you want to know what is the next specified weekend day in your applications.
#### Example:
```csharp

var dateVariable1 = DateTime.Today;
dateVariable1.GetUpcoming(DayOfWeek.Monday); // returns the next Monday date          
dateVariable1.GetUpcoming(DayOfWeek.Monday,true); // returns the next Monday date, if today is Monday, it returns the Monday date of next week.   
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
DateTime.Today.AddDays(-1).IsAfter();// returns false
DateTime.Today.AddDays(1).IsAfter();// returns true
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
DateTime.Today.IsAfterOrEqualTo(DateTime.Today);//returns True
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
DateTime.Today.AddDays(-1).IsBefore();// returns true
DateTime.Today.AddDays(1).IsBefore();// returns false
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
DateTime.Today.IsBeforeOrEqualTo(DateTime.Today);// returns True
```
 ## IsBetween( {StartDateTime}, {EndDateTime} , {includingEdges})
Determines whether this date is between two sepcified dates.
#### When to use it?
When you want to know a specific date is between two other dates in your application. 
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 8); 
dateVariable1.IsBetween(new DateTime(2017,10,10), new DateTime(2019,10,10));// returns "True"
dateVariable1.IsBetween(new DateTime(2017,10,10), new DateTime(2016,10,10)); // throw new ArgumentException("\"From\" date should be smaller than or equal to \"To\" date.")
dateVariable1.IsBetween(new DateTime(2017,10,10), new DateTime(2017,11,10));// returns "False"
```    
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
## IsInTheFuture()
Determines whether this date is in the future.
- It returns `true` if the date after  `LocalTime.Today`.
- It returns `false` if the date is today.
#### When to use it?
When you want to know a DateTime object is greater than today or not in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 5, 5); 
dateVariable1.IsInTheFuture(); // returns false

var dateVariable2 = DateTime.Today; 
dateVariable2.IsInTheFuture(); // returns false
DateTime.Today.AddDays(-1).IsInTheFuture();// returns true
DateTime.Today.AddDays(1).IsInTheFuture();// returns false
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
dateVariable2.IsInThePast(); // returns false
DateTime.Today.AddDays(-1).IsInThePast();// returns true
DateTime.Today.AddDays(1).IsInThePast();// returns false
```
## IsLastDayOfMonth()
- Returns `true` if the date is the last day of its month.
- Returns `false` if the date is NOT the last day of its month.
#### When to use it?
When you want to know whether the is the last day of its month in your application. 
For example, in Payroll system, you should have the last date of each Month to calclulate amount of the wage of that month.

The last day of each month:
- January - 31 
- February - 28 / 29 in leap year.
- March - 31 
- April - 30 
- May - 31 
- June - 30
- July - 31
- August - 31
- September - 31
- October - 31
- November - 30
- December - 31

#### Example:

```csharp

new DateTime(2018,10,09).IsLastDayOfMonth().ToString(); // returns "false"
new DateTime(2018,10,31).IsLastDayOfMonth().ToString(); // returns "true"
new DateTime(2020,02,28).IsLastDayOfMonth().ToString(); // returns "false" , leap year
new DateTime(2020,02,29).IsLastDayOfMonth().ToString(); // returns "true" , leap year
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
DateTime.Today.IsNewerThan(TimeSpan.FromDays(0));// returns False
```

## IsOlderThan({TimeSpan})
- It returns `True` if this date is older than {TimeSpan}.
- It returns `false` if this date is not older than {TimeSpan}.
#### When to use it?
When you want to know whether the date is older than another date or not.
#### Example:
```csharp
var dateVariable1 = DateTime.Today;
dateVariable1.IsOlderThan(TimeSpan.FromDays(1)); // returns False because tomorrow is not older than today.
dateVariable1.IsOlderThan(TimeSpan.FromDays(-1)); // returns True because yesterday is older than today.
DateTime.Today.IsOlderThan(TimeSpan.FromDays(0));// returns "True"
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
DateTime.Today.AddDays(-1).IsToday();// returns false
DateTime.Today.AddDays(1).IsToday();// returns false
```

## IsTodayOrFuture()
Determines whether this date is in the future or today.
- It returns `true` if the date after or equal to `LocalTime.Today`.
- It returns `false` if the date is in the past.
#### When to use it?
When you want to know a DateTime object is today or greater than today  in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 5, 5); 
dateVariable1.IsTodayOrFuture(); // returns false

var dateVariable2 = DateTime.Today; 
dateVariable2.IsTodayOrFuture(); // returns true
DateTime.Today.AddDays(-1).IsTodayOrFuture();// returns false
DateTime.Today.AddDays(1).IsTodayOrFuture();// returns true
```
## IsWeekend()
Determines whether this date is weekend or not (Saturday or Sunday).
- It returns `true` if this date is Saturday or Sunday.
#### When to use it?
When you want to know a DateTime object is weekend in your application.
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 7);
dateVariable2.IsWeekend(); // returns true because this day is Sunday

var dateVariable2 = new DateTime(2018, 10, 8);
dateVariable2.IsWeekend(); // returns false because this day is Monday
```


       
## Min({otherdate})
Gets the minimum value between this date and {otherdate}.
#### When to use it?
When you want to know which of two date is minimum.

#### Example:

```csharp

new DateTime(2020,10,12).Min(new DateTime(2020,10,12)).ToString(); //returns "10/12/2020 12:00:00 AM"
new DateTime(2020,10,12).Min(new DateTime(2018,10,12)).ToString(); //returns "10/12/2018 12:00:00 AM"
new DateTime(2018,10,12).Min(new DateTime(2019,5,1)).ToString();   //returns "10/12/2018 12:00:00 AM"
new DateTime(1,10,12).Min(new DateTime(1,5,1)).ToString();         //returns "5/01/0001 12:00:00 AM"
new DateTime(1,5,12).Min(new DateTime(1,12,1)).ToString();         //returns "5/12/0001 12:00:00 AM"
new DateTime(9999,5,12).Min(new DateTime(1,12,1)).ToString();      //returns "12/01/0001 12:00:00 AM"

```          
    
## Max({otherdate})
Gets the maximun value between this date and {otherdate}.
#### When to use it?
When you want to know which of two date is maximun.

#### Example:

```csharp

new DateTime(2020,10,12).Max(new DateTime(2020,10,12)).ToString(); //returns "10/12/2020 12:00:00 AM"
new DateTime(2020,10,12).Max(new DateTime(2018,10,12)).ToString(); //returns "10/12/2020 12:00:00 AM"
new DateTime(2018,10,12).Max(new DateTime(2019,5,1)).ToString();   //returns "5/01/2019 12:00:00 AM"
new DateTime(1,10,12).Max(new DateTime(1,5,1)).ToString();         //returns "10/12/0001 12:00:00 AM"
new DateTime(1,5,12).Max(new DateTime(1,12,1)).ToString();         //returns "12/01/0001 12:00:00 AM"
new DateTime(9999,5,12).Max(new DateTime(1,12,1)).ToString();      //returns "5/12/9999 12:00:00 AM"

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
When you want to know the previous working day in your application.
#### Example:

```csharp

var dateVariable1 = new DateTime(2018, 10, 8); // Monday
dateVariable2.PreviousWorkingDay(); // returns "05/10/2018 12:00:00 AM" Friday

var dateVariable2 = new DateTime(2018, 10, 9); // Teusday
dateVariable2.PreviousWorkingDay(); // returns "08/10/2018 12:00:00 AM" next Monday
```
## Round({nearestTimeSpan})
Rounds this up to the nearest interval (e.g. second, minute, hour, etc).
#### When to use it?
When you want to round this date value up to the nearest timespan in your applications.

#### Example:

```csharp

new DateTime(2018,10,12,1,1,1).Round(TimeSpan.FromHours(2)).ToString(); //returns "10/12/2018 2:00:00 AM"
new DateTime(2018,10,12,1,1,1).Round(TimeSpan.FromHours(3)).ToString(); //returns "10/12/2018 12:00:00 AM"
new DateTime(2018,10,12,1,1,1).Round(TimeSpan.FromHours(4)).ToString(); //returns "10/12/2018 12:00:00 AM"
new DateTime(2018,10,12,1,1,1).Round(TimeSpan.FromHours(5)).ToString(); //returns "10/12/2018 3:00:00 AM"
new DateTime(2018,10,12,1,1,1).Round(TimeSpan.FromHours(6)).ToString(); //returns "10/12/2018 12:00:00 AM"
new DateTime(2018,10,12,10,1,1).Round(TimeSpan.FromHours(1)).ToString();//returns "10/12/2018 10:00:00 AM"
new DateTime(2018,10,12,10,1,1).Round(TimeSpan.FromHours(2)).ToString();//returns "10/12/2018 10:00:00 AM"
new DateTime(2018,10,12,10,1,1).Round(TimeSpan.FromHours(3)).ToString();//returns "10/12/2018 9:00:00 AM"
new DateTime(2018,10,12,10,1,1).Round(TimeSpan.FromHours(4)).ToString();//returns "10/12/2018 12:00:00 PM"
new DateTime(2018,10,12,10,1,1).Round(TimeSpan.FromHours(5)).ToString();//returns "10/12/2018 8:00:00 AM"
```   

## RoundToSecond()
Rounds this up to the nearest whole second.
If `milisecond` part of time is equal or greater than 500, the second part is rounded to upper second. Otherwise, it is rounded to the its second.
#### When to use it?
When you want to round this date value up to the nearest whole second in your applications.

#### Example:

```csharp
new DateTime(2018,10,12,12,15,43,850).RoundToSecond().ToString(); // returns "10/12/2018 12:15:44 PM"
new DateTime(2018,10,12,12,15,43,55).RoundToSecond().ToString();  // returns "10/12/2018 12:15:43 PM"
new DateTime(2018,10,12,12,15,43,05).RoundToSecond().ToString();  // returns "10/12/2018 12:15:43 PM"
new DateTime(2018,10,12,12,15,43,0).RoundToSecond().ToString();   // returns "10/12/2018 12:15:43 PM"
new DateTime(2018,10,12,12,15,43,500).RoundToSecond().ToString(); // returns "10/12/2018 12:15:44 PM"
new DateTime(2018,10,12,12,15,43,499).RoundToSecond().ToString(); // returns "10/12/2018 12:15:43 PM"
new DateTime(2018,10,12,12,15,43,501).RoundToSecond().ToString(); // returns "10/12/2018 12:15:44 PM"

```   

## RoundToMinute()
Rounds this up to the nearest whole minute.
If `second` part of time is equal or greater than 30, the minute part is rounded to upper minute. Otherwise, it is rounded to the its minute.
#### When to use it?
When you want to round this date value up to the nearest whole minute in your applications.

#### Example:

```csharp

new DateTime(2018,10,12,12,15,43).RoundToMinute().ToString(); // returns "10/12/2018 12:16:00 PM" 
new DateTime(2018,10,12,12,15,05).RoundToMinute().ToString(); // returns "10/12/2018 12:15:00 PM"
new DateTime(2018,10,12,12,15,30).RoundToMinute().ToString(); // returns  "10/12/2018 12:16:00 PM"
new DateTime(2018,10,12,12,15,29).RoundToMinute().ToString(); // returns "10/12/2018 12:15:00 PM"
new DateTime(2018,10,12,12,15,31).RoundToMinute().ToString(); // returns "10/12/2018 12:16:00 PM"
```   

## RoundToHour()
Rounds this up to the nearest whole hour.
If `minute` part of time is equal or greater than 30, the hour part is rounded to upper o'clock. Otherwise, it is rounded to the current o'clock.
#### When to use it?
When you want to round this date value up to the nearest whole minute in your applications.

#### Example:

```csharp
new DateTime(2018,10,12,05,29,05).RoundToHour().ToString(); // returns "10/12/2018 5:00:00 AM"
new DateTime(2018,10,12,05,30,05).RoundToHour().ToString(); // returns "10/12/2018 6:00:00 AM"
new DateTime(2018,10,12,05,31,05).RoundToHour().ToString(); // returns "10/12/2018 6:00:00 AM"
```   
 ## ToFriendlyDateString()
Gets useful format of the date.
#### When to use it?
When you want to show the date and time in a good shape in your application. 
#### Example:

```csharp
// Today is 08/10/2018 - Tuesday
var dateVariable1 = new DateTime(2018, 10, 8); 
dateVariable1.ToFriendlyDateString(); //returns  "Today @ 10:55 am"

var dateVariable2 = DateTime.Today.AddDays(-1);
dateVariable2.ToFriendlyDateString(); // returns "Yesterday @ 12:00 am"

var dateVariable3 = DateTime.Today.AddDays(2);
dateVariable3.ToFriendlyDateString(); // returns "Thursday @ 12:00 am"
```
## ToLocal({TimeZoneInfo})
Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.TimeZoneProvider.
Use this instead of ToLocalTime() so you get control over the TimeZone.
If the value is `Null`, it returns difference between Local and UTC date value.
#### When to use it?
When you want to know the local time equivalent of this UTC date value in your applications.

#### Example:

```csharp
// Current location is London
new DateTime(2018,10,12).ToLocal().ToString(); // returns "10/12/2018 1:00:00 AM" 

DateTime? dateVariable3 = new DateTime();
dateVariable3.ToLocal().ToString(); // returns "1/1/0001 1:00:00 AM"

new DateTime(2018,10,12).ToLocal(TimeZoneInfo.Local).ToString(); // returns "10/12/2018 1:00:00 AM"

new DateTime(2018,10,12).ToLocal(TimeZoneInfo.Utc).ToString(); // returns "10/12/2018 12:00:00 AM"

new DateTime(2018,10,12).ToLocal(TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")); // returns "10/11/2018 7:00:00 PM"
new DateTime(2018,10,12).ToLocal(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); // returns "10/11/2018 8:00:00 PM"
```          
## ToSmallTime()
It shows the time as hh:mm AM or hh:mm PM.
#### When to use it?
When you want to show the time in a good shape in your application.
#### Example:

```csharp

var dateVariable1 = DateTime.Now;
dateVariable2.ToSmallTime(); // returns e.g: 2:15pm or 1:43am
```
 ## Tostring({formatstring})
Gets this value as `string` of date.
> You can set the output format by {formatstring}.
1.  **d**  -> Represents the day of the month as a number from 1 through 31.  
      
    
2.  **dd**  -> Represents the day of the month as a number from 01 through 31.  
      
    
3.  **ddd**-> Represents the abbreviated name of the day (Mon, Tues, Wed etc).  
      
    
4.  **dddd**-> Represents the full name of the day (Monday, Tuesday etc).  
      
    
5.  **h**  12-hour clock hour (e.g. 4).  
      
    
6.  **hh**  12-hour clock, with a leading 0 (e.g. 06)  
      
    
7.  **H**  24-hour clock hour (e.g. 15)  
      
    
8.  **HH**  24-hour clock hour, with a leading 0 (e.g. 22)  
      
    
9.  **m**  Minutes  
      
    
10.  **mm**  Minutes with a leading zero  
      
    
11.  **M**  Month number(eg.3)  
      
    
12.  **MM**  Month number with leading zero(eg.04)  
      
    
13.  **MMM**  Abbreviated Month Name (e.g. Dec)  
      
    
14.  **MMMM**  Full month name (e.g. December)  
      
    
15.  **s**  Seconds  
      
    
16.  **ss**  Seconds with leading zero  
      
    
17.  **t**  Abbreviated AM / PM (e.g. A or P)  
      
    
18.  **tt**  AM / PM (e.g. AM or PM  
 19.  **y**  Year, no leading zero (e.g. 2015 would be 15)  
      
    
20.  **yy**  Year, leadin zero (e.g. 2015 would be 015)  
      
    
21.  **yyy**  Year, (e.g. 2015)  
22.  **yyyy**  Year, (e.g. 2015)  


#### When to use it?
When you want to show date as a `string` according to your needs in your application. 
#### Example:

```csharp
DateTime(2018, 10, 9).ToString();// returns "10/9/2018 12:00:00 AM"
DateTime? dateVariable3 = new DateTime();
dateVariable3.ToString();// returns "1/1/0001 12:00:00 AM"
DateTime? dateVariable4 = null;
dateVariable3.ToString();// returns ""
DateTime.Now.ToString("MM/dd/yyyy");// returns 05/29/2015
DateTime.Now.ToString("dddd, dd MMMM yyyy");// returns Friday, 29 May 2015
DateTime.Now.ToString("dddd, dd MMMM yyyy");// returns Friday, 29 May 2015 05:50
DateTime.Now.ToString("dddd, dd MMMM yyyy");// returns Friday, 29 May 2015 05:50 AM
DateTime.Now.ToString("dddd, dd MMMM yyyy");// returns Friday, 29 May 2015 5:50
DateTime.Now.ToString("dddd, dd MMMM yyyy");// returns Friday, 29 May 2015 5:50 AM
DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");// returns Friday, 29 May 2015 05:50:06
DateTime.Now.ToString("MM/dd/yyyy HH:mm");// returns 05/29/2015 05:50
DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");// returns 05/29/2015 05:50 AM
DateTime.Now.ToString("MM/dd/yyyy H:mm");// returns 05/29/2015 5:50
DateTime.Now.ToString("MM/dd/yyyy h:mm tt");// returns 05/29/2015 5:50 AM
DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");// returns 05/29/2015 05:50:06
DateTime.Now.ToString("MMMM dd");// returns May 29
DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK");// returns 2015-05-16T05:50:06.7199222-04:00
DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’");// returns Fri, 16 May 2015 05:50:06 GMT
DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss");// returns 2015-05-16T05:50:06
DateTime.Now.ToString("HH:mm");// returns 05:50
DateTime.Now.ToString("hh:mm tt");// returns 05:50 AM
DateTime.Now.ToString("H:mm");// returns 5:50
DateTime.Now.ToString("h:mm tt");// returns 5:50 AM
DateTime.Now.ToString("HH:mm:ss");// returns 05:50:06
DateTime.Now.ToString("yyyy MMMM");// returns 2015 May
```

 ## ToTimeDifferenceString({precisionParts},{longForm})
Gets the difference day and time to now.
#### When to use it?
When you want to know the difference day and time to now in your application. 
- You can set {precisionParts} and {longForm} to set output format. You can see all in samples section.
#### Example:

```csharp
var dateVariable1 = new DateTime(2018, 10, 9); // runtime of samples is 9/10/2018
dateVariable1.ToTimeDifferenceString(); //returns "10 hours and 35 minutes ago"
dateVariable1.ToTimeDifferenceString(0); //returns " ago"
dateVariable1.ToTimeDifferenceString(1); //returns "10 hours ago"
dateVariable1.ToTimeDifferenceString(2); //returns "10 hours and 35 minutes ago"
dateVariable1.ToTimeDifferenceString(3); //returns "10 hours, 35 minutes and 47 seconds ago"
dateVariable1.ToTimeDifferenceString(false); //returns "10h 35m ago"
dateVariable1.ToTimeDifferenceString(true); //returns "10 hours and 36 minutes ago"
dateVariable1.ToTimeDifferenceString(0,false); //returns " ago"
dateVariable1.ToTimeDifferenceString(1,false); //returns "10h ago"
dateVariable1.ToTimeDifferenceString(2,false); //returns "10h 37m ago"
dateVariable1.ToTimeDifferenceString(3,false); //returns "10h 37m 50s ago"
dateVariable1.ToTimeDifferenceString(3,true); //returns "10 hours, 38 minutes and 1 second ago"
dateVariable1.ToTimeDifferenceString(0,true); //returns " ago"
dateVariable1.ToTimeDifferenceString(1,true); //returns "10 hours ago"
dateVariable1.ToTimeDifferenceString(2,true); //returns "10 hours and 38 minutes ago"
dateVariable1.ToTimeDifferenceString(3,true); //returns "10 hours, 38 minutes and 49 seconds ago"

var dateVariable2 = DateTime.Now;
dateVariable2.ToTimeDifferenceString(); //returns "Just Now"
dateVariable2.ToTimeDifferenceString(false); //returns "Now"

var dateVariable3 = DateTime.Today.AddDays(2);
dateVariable1.ToTimeDifferenceString(); // returns "1 day and 13 hours later"
```
   
   ## ToUniversal({TimeZoneInfo})
Returns the equivalent Universal Time (UTC) of this local date value.
#### When to use it?
When you want to know the local time equivalent of this UTC date value in your applications.

#### Example:

```csharp
// Current location is London
new DateTime(2018,10,12).ToUniversal(); //returns "10/11/2018 8:30:00 PM"
new DateTime(2018,10,12).ToUniversal(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); //returns "10/12/2018 5:00:00 AM"
new DateTime(2018,10,12).ToUniversal(TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")); // returns "10/12/2018 6:00:00 AM"
```    
## ToUnixTime()
Gets the total number of seconds elapsed since 1st Jan 1970.
#### When to use it?
The `unix time stamp` is a way to track time as a running total of seconds. This count starts at the `Unix Epoch` on January 1st, 1970 at UTC. Therefore, the `unix time stamp` is merely the number of seconds between a particular date and the `Unix Epoch`. It should also be pointed out that this point in time technically does not change no matter where you are located on the globe. This is very useful to computer systems for tracking and sorting dated information in dynamic and distributed applications both online and client side.
On `January 19, 2038` the `Unix Time Stamp` will cease to work due to a 32-bit overflow. Before this moment millions of applications will need to either adopt a new convention for time stamps or be migrated to 64-bit systems which will buy the time stamp a "bit" more time(www.unixtimestamp.com).
#### Example:

```csharp

new DateTime(2018,10,12).ToUnixTime().ToString(); // returns "1539289800"
new DateTime(1970,1,1).ToUnixTime().ToString(); //returns "-12600"
new DateTime(1970,2,1).ToUnixTime().ToString(); //returns "2665800"
new DateTime(1970,1,2).ToUnixTime().ToString(); //returns "73800"
new DateTime(1970,1,1,3,30,0,0).ToUnixTime().ToString(); //returns "0"
```
