using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive._Extensions
{
    internal class UKHoliday
    {
        String[,] EsterTable;

        public UKHoliday()
        {
            EsterTable = EasterSundayTable.GetTable();
        }

        public bool IsUkHoliday(DateTime date)
        {
            var _IsNewYearHoliday = IsNewYearHoliday(date);
            var _IsGoodFriday = IsGoodFriday(date);
            var _IsEasterSunday = IsEasterSunday(date);
            var _IsEasterMonday = IsEasterMonday(date);
            var _IsAugBankHoliday = IsAugBankHoliday(date);
            var _IsMayBankHoliday = IsMayBankHoliday(date);
            var _IsMayDayHoliday = IsMayDayHoliday(date);
            var _IsChristmassHoliday = IsChristmassHoliday(date);
            var _IsBoxingDayHoliday = IsBoxingDayHoliday(date);
            var _IsInChristmassHolidays = IsInChristmassHolidays(date);
            var _IsSpecialHoliday = IsSpecialHoliday(date);

            return
                _IsNewYearHoliday ||
                _IsGoodFriday ||
                _IsEasterSunday ||
                _IsEasterMonday ||
                _IsAugBankHoliday ||
                _IsMayBankHoliday ||
                _IsMayDayHoliday ||
                _IsChristmassHoliday ||
                _IsBoxingDayHoliday ||
                _IsInChristmassHolidays ||
                _IsSpecialHoliday;
        }


        public List<Holiday> GetYearHolidays(int year)
        {
            var result = new List<Holiday>();
            result.Add(new Holiday() { Date = GetNewYearHolidayDate(year), Name = "New Year's Day" });
            result.Add(new Holiday() { Date = GetGoodFridayDate(year), Name = "Good Friday" });
            result.Add(new Holiday() { Date = GetEasterSundayDate(year), Name = "Easter Sunday" });
            result.Add(new Holiday() { Date = GetEasterMondayDate(year), Name = "Easter Monday" });
            result.Add(new Holiday() { Date = GetMayDayDate(year), Name = "May Day" });
            result.Add(new Holiday() { Date = GetMayBankHoliday(year), Name = "Bank Holiday" });
            result.Add(new Holiday() { Date = GetAugBankHoliday(year), Name = "Bank Holiday" });
            result.Add(new Holiday() { Date = GetChristmassHolidayDate(year), Name = "Christmas Day" });
            result.Add(new Holiday() { Date = GetBoxingDayDate(year), Name = "Boxing Day" });
            var christmassholidays = GetChristmassHolidayDates(year);
            for (int i = 2; i < christmassholidays.Count; ++i)
            {
                result.Add(new Holiday() { Date = christmassholidays[i], Name = "Christmass Holiday" });
            }

            return result;
        }

        public bool IsInChristmassHolidays(DateTime date)
        {
            var christmassholidays = GetChristmassHolidayDates(date.Year);
            bool result = false;
            foreach (var item in christmassholidays)
            {
                if (item.Month == date.Month && item.Day == date.Day)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool IsGoodFriday(DateTime date)
        {
            try
            {
                if (date == null) throw new Exception("input date is null");
                if (date.DayOfWeek != DayOfWeek.Friday) return false;
                var goodFridaydate = GetGoodFridayDate(date.Year);
                return goodFridaydate.Month == date.Month && goodFridaydate.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsGoodFriday function error because {ex.Message}");
            }
        }

        public bool IsEasterSunday(DateTime date)
        {
            try
            {
                if (date == null) throw new Exception("input date is null");
                if (date.DayOfWeek != DayOfWeek.Sunday) return false;
                var eastersundaydate = GetEsterdateOfYear(date.Year);
                return eastersundaydate.Month == date.Month && eastersundaydate.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsEasterSunday function error because {ex.Message}");
            }
        }

        public bool IsEasterMonday(DateTime date)
        {
            try
            {
                if (date == null) throw new Exception("input date is null");
                if (date.DayOfWeek != DayOfWeek.Monday) return false;
                var eastermondaydate = GetEasterMondayDate(date.Year);
                return eastermondaydate.Month == date.Month && eastermondaydate.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsEasterMonday function error because {ex.Message}");
            }
        }

        public bool IsNewYearHoliday(DateTime date)
        {
            try
            {
                var newYearHolidaydate = GetNewYearHolidayDate(date.Year);
                return newYearHolidaydate.Month == date.Month && newYearHolidaydate.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsNewYearHoliday function error because {ex.Message}");
            }
        }

        public bool IsAugBankHoliday(DateTime date)
        {
            try
            {
                var augBankHoliday = GetAugBankHoliday(date.Year);
                return augBankHoliday.Month == date.Month && augBankHoliday.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsAugBankHoliday function error because {ex.Message}");
            }
        }

        public bool IsMayBankHoliday(DateTime date)
        {
            try
            {
                var mayBankHoliday = GetMayBankHoliday(date.Year);
                return mayBankHoliday.Month == date.Month && mayBankHoliday.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsMayBankHoliday function error because {ex.Message}");
            }
        }

        public bool IsMayDayHoliday(DateTime date)
        {
            try
            {
                var mayDayHoliday = GetMayDayDate(date.Year);
                return mayDayHoliday.Month == date.Month && mayDayHoliday.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsMayBankHoliday function error because {ex.Message}");
            }
        }

        public bool IsChristmassHoliday(DateTime date)
        {
            try
            {
                var ChristmassHoliday = GetChristmassHolidayDate(date.Year);
                return ChristmassHoliday.Month == date.Month && ChristmassHoliday.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsMayBankHoliday function error because {ex.Message}");
            }
        }

        public bool IsBoxingDayHoliday(DateTime date)
        {
            try
            {
                var boxingDayDate = GetBoxingDayDate(date.Year);
                return boxingDayDate.Month == date.Month && boxingDayDate.Day == date.Day;

            }
            catch (Exception ex)
            {
                throw new Exception($"IsMayBankHoliday function error because {ex.Message}");
            }
        }

        public bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public bool IsSpecialHoliday(DateTime date)
        {
            var specialholydays = GetSpecialHolidays(date.Year);
            return specialholydays.Any(x => x.Month == date.Month && x.Day == date.Day);
        }

        public DateTime GetGoodFridayDate(int year)
        {
            return GetEsterdateOfYear(year).AddDays(-2);
        }

        public DateTime GetEasterMondayDate(int year)
        {
            return GetEsterdateOfYear(year).AddDays(1);
        }

        public DateTime GetEasterSundayDate(int year)
        {
            return GetEsterdateOfYear(year);
        }

        public DateTime GetNewYearHolidayDate(int year)
        {
            DateTime result = new DateTime(year, 1, 1);
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result = result.AddDays(1);
            return result;
        }

        public DateTime GetAugBankHoliday(int year)
        {
            DateTime result = new DateTime(year, 8, DateTime.DaysInMonth(year, 8));
            while (result.DayOfWeek != DayOfWeek.Monday)
                result = result.AddDays(-1);
            return result;
        }

        public DateTime GetMayBankHoliday(int year)
        {
            DateTime result = new DateTime(year, 5, DateTime.DaysInMonth(year, 5));
            while (result.DayOfWeek != DayOfWeek.Monday)
                result = result.AddDays(-1);
            return result;
        }

        public DateTime GetMayDayDate(int year)
        {
            DateTime result = new DateTime(year, 5, 1);
            while (result.DayOfWeek != DayOfWeek.Monday)
                result = result.AddDays(1);
            return result;
        }

        public DateTime GetChristmassHolidayDate(int year)
        {
            DateTime result = new DateTime(year, 12, 25);
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result = result.AddDays(1);
            return result;
        }

        public DateTime GetBoxingDayDate(int year)
        {
            var boxingday = GetChristmassHolidayDate(year).AddDays(1);
            while (boxingday.DayOfWeek == DayOfWeek.Saturday || boxingday.DayOfWeek == DayOfWeek.Sunday)
                boxingday = boxingday.AddDays(1);
            return boxingday;
        }

        public List<DateTime> GetChristmassHolidayDates(int year)
        {
            var result = new List<DateTime>();
            var Christmassday = GetChristmassHolidayDate(year);
            var boxingDay = GetBoxingDayDate(year);
            result.Add(Christmassday);
            result.Add(boxingDay);
            if (Christmassday.DayOfWeek == DayOfWeek.Saturday || Christmassday.DayOfWeek == DayOfWeek.Sunday || boxingDay.DayOfWeek == DayOfWeek.Saturday || boxingDay.DayOfWeek == DayOfWeek.Sunday)
            {
                result.Add(boxingDay.AddDays(1));
            }

            return result;
        }




        public List<DateTime> GetSpecialHolidays(int year)
        {
            var result = new List<DateTime>();
            result.Add(new DateTime(2012, 06, 04)); // Based on the old data
            result.Add(new DateTime(2012, 06, 05)); // Based on the old data
            result.Add(new DateTime(2020, 05, 08)); // Early May Bank Holiday
            result.Add(new DateTime(2022, 06, 02)); // Based on the old data
            result.Add(new DateTime(2022, 06, 03)); // Platinum Jubilee
            result.Add(new DateTime(2022, 09, 19)); // Bank Holiday for the State Funeral of Queen Elizabeth II
            return result.Where(x => x.Year == year).ToList();
        }

        DateTime GetEsterdateOfYear(int year)
        {
            try
            {
                int yearofdate = year;
                int x = yearofdate % 19;
                int y = -1;

                if (yearofdate >= 326 && yearofdate <= 1582)
                {
                    y = 0;
                }
                else if (yearofdate >= 1583 && yearofdate <= 1699)
                {
                    y = 1;
                }
                else if (yearofdate >= 1700 && yearofdate <= 1899)
                {
                    y = 2;
                }
                else if (yearofdate >= 1900 && yearofdate <= 2199)
                {
                    y = 3;
                }
                else if ((yearofdate >= 2200 && yearofdate <= 2299) || (yearofdate >= 2400 && yearofdate <= 2499))
                {
                    y = 4;
                }
                else if ((yearofdate >= 2300 && yearofdate <= 2399) || (yearofdate >= 2500 && yearofdate <= 2599))
                {
                    y = 5;
                }
                else if (yearofdate >= 2600 && yearofdate <= 2899)
                {
                    y = 6;
                }
                else if (yearofdate >= 2900 && yearofdate <= 3099)
                {
                    y = 7;
                }
                else if (yearofdate >= 3100 && yearofdate <= 3399)
                {
                    y = 8;
                }
                else if ((yearofdate >= 3400 && yearofdate <= 3499) || (yearofdate >= 3600 && yearofdate <= 3699))
                {
                    y = 9;
                }
                else if ((yearofdate >= 3500 && yearofdate <= 3599) || (yearofdate >= 3700 && yearofdate <= 3799))
                {
                    y = 10;
                }
                else if (yearofdate >= 3800 && yearofdate <= 4099)
                {
                    y = 11;
                }
                else
                {
                    throw new Exception($"Accepted year range is : 326 to 4099");
                }

                var monthandday = EsterTable[x, y].Split('-');

                var month = Convert.ToInt32(monthandday[0]);
                var day = Convert.ToInt32(monthandday[1]);
                var aValue = EasterSundayTable.GetA(month, day);
                var bValue = EasterSundayTable.GetB(year);
                var cValue = EasterSundayTable.GetC(year);
                var dValue = EasterSundayTable.GetD(aValue + bValue + cValue);
                return new DateTime(year, month, day).AddDays(dValue);
            }
            catch
            {
                throw;
            }
        }

    }

    static class EasterSundayTable
    {
        public static String[,] GetTable()
        {
            string[,] EasterTable = new string[19, 12];
            EasterTable[00, 00] = "04-05";
            EasterTable[00, 01] = "04-12";
            EasterTable[00, 02] = "04-13";
            EasterTable[00, 03] = "04-14";
            EasterTable[00, 04] = "04-15";
            EasterTable[00, 05] = "04-16";
            EasterTable[00, 06] = "04-17";
            EasterTable[00, 07] = "04-18";
            EasterTable[00, 08] = "04-18";
            EasterTable[00, 09] = "03-21";
            EasterTable[00, 10] = "03-22";
            EasterTable[00, 11] = "03-23";

            EasterTable[01, 00] = "03-25";
            EasterTable[01, 01] = "04-01";
            EasterTable[01, 02] = "04-02";
            EasterTable[01, 03] = "04-03";
            EasterTable[01, 04] = "04-04";
            EasterTable[01, 05] = "04-05";
            EasterTable[01, 06] = "04-06";
            EasterTable[01, 07] = "04-07";
            EasterTable[01, 08] = "04-08";
            EasterTable[01, 09] = "04-09";
            EasterTable[01, 10] = "04-10";
            EasterTable[01, 11] = "04-11";

            EasterTable[02, 00] = "04-13";
            EasterTable[02, 01] = "03-21";
            EasterTable[02, 02] = "03-22";
            EasterTable[02, 03] = "03-23";
            EasterTable[02, 04] = "03-24";
            EasterTable[02, 05] = "03-25";
            EasterTable[02, 06] = "03-26";
            EasterTable[02, 07] = "03-27";
            EasterTable[02, 08] = "03-28";
            EasterTable[02, 09] = "03-29";
            EasterTable[02, 10] = "03-30";
            EasterTable[02, 11] = "03-31";

            EasterTable[03, 00] = "04-02";
            EasterTable[03, 01] = "04-09";
            EasterTable[03, 02] = "04-10";
            EasterTable[03, 03] = "04-11";
            EasterTable[03, 04] = "04-12";
            EasterTable[03, 05] = "04-13";
            EasterTable[03, 06] = "04-14";
            EasterTable[03, 07] = "04-15";
            EasterTable[03, 08] = "04-16";
            EasterTable[03, 09] = "04-17";
            EasterTable[03, 10] = "04-18";
            EasterTable[03, 11] = "04-18";

            EasterTable[04, 00] = "03-22";
            EasterTable[04, 01] = "03-29";
            EasterTable[04, 02] = "03-30";
            EasterTable[04, 03] = "03-31";
            EasterTable[04, 04] = "04-01";
            EasterTable[04, 05] = "04-02";
            EasterTable[04, 06] = "04-03";
            EasterTable[04, 07] = "04-04";
            EasterTable[04, 08] = "04-05";
            EasterTable[04, 09] = "04-06";
            EasterTable[04, 10] = "04-07";
            EasterTable[04, 11] = "04-08";

            EasterTable[05, 00] = "04-10";
            EasterTable[05, 01] = "04-17";
            EasterTable[05, 02] = "04-18";
            EasterTable[05, 03] = "04-18";
            EasterTable[05, 04] = "03-21";
            EasterTable[05, 05] = "03-22";
            EasterTable[05, 06] = "03-23";
            EasterTable[05, 07] = "03-24";
            EasterTable[05, 08] = "03-25";
            EasterTable[05, 09] = "03-26";
            EasterTable[05, 10] = "03-27";
            EasterTable[05, 11] = "03-28";

            EasterTable[06, 00] = "03-30";
            EasterTable[06, 01] = "04-06";
            EasterTable[06, 02] = "04-07";
            EasterTable[06, 03] = "04-08";
            EasterTable[06, 04] = "04-09";
            EasterTable[06, 05] = "04-10";
            EasterTable[06, 06] = "04-11";
            EasterTable[06, 07] = "04-12";
            EasterTable[06, 08] = "04-13";
            EasterTable[06, 09] = "04-14";
            EasterTable[06, 10] = "04-15";
            EasterTable[06, 11] = "04-16";

            EasterTable[07, 00] = "04-18";
            EasterTable[07, 01] = "03-26";
            EasterTable[07, 02] = "03-17";
            EasterTable[07, 03] = "03-28";
            EasterTable[07, 04] = "03-29";
            EasterTable[07, 05] = "03-30";
            EasterTable[07, 06] = "03-31";
            EasterTable[07, 07] = "04-01";
            EasterTable[07, 08] = "04-02";
            EasterTable[07, 09] = "04-03";
            EasterTable[07, 10] = "04-04";
            EasterTable[07, 11] = "04-05";

            EasterTable[08, 00] = "04-07";
            EasterTable[08, 01] = "04-14";
            EasterTable[08, 02] = "04-15";
            EasterTable[08, 03] = "04-16";
            EasterTable[08, 04] = "04-17";
            EasterTable[08, 05] = "04-18";
            EasterTable[08, 06] = "04-18";
            EasterTable[08, 07] = "03-21";
            EasterTable[08, 08] = "03-22";
            EasterTable[08, 09] = "03-23";
            EasterTable[08, 10] = "03-24";
            EasterTable[08, 11] = "03-25";

            EasterTable[09, 00] = "03-27";
            EasterTable[09, 01] = "04-03";
            EasterTable[09, 02] = "04-04";
            EasterTable[09, 03] = "04-05";
            EasterTable[09, 04] = "04-06";
            EasterTable[09, 05] = "04-07";
            EasterTable[09, 06] = "04-08";
            EasterTable[09, 07] = "04-09";
            EasterTable[09, 08] = "04-10";
            EasterTable[09, 09] = "04-11";
            EasterTable[09, 10] = "04-12";
            EasterTable[09, 11] = "04-13";

            EasterTable[10, 00] = "04-15";
            EasterTable[10, 01] = "03-23";
            EasterTable[10, 02] = "03-24";
            EasterTable[10, 03] = "03-25";
            EasterTable[10, 04] = "03-26";
            EasterTable[10, 05] = "03-27";
            EasterTable[10, 06] = "03-28";
            EasterTable[10, 07] = "03-29";
            EasterTable[10, 08] = "03-30";
            EasterTable[10, 09] = "03-31";
            EasterTable[10, 10] = "04-01";
            EasterTable[10, 11] = "04-02";

            EasterTable[11, 00] = "04-04";
            EasterTable[11, 01] = "04-11";
            EasterTable[11, 02] = "04-12";
            EasterTable[11, 03] = "04-13";
            EasterTable[11, 04] = "04-14";
            EasterTable[11, 05] = "04-15";
            EasterTable[11, 06] = "04-16";
            EasterTable[11, 07] = "04-17";
            EasterTable[11, 08] = "04-17";
            EasterTable[11, 09] = "04-18";
            EasterTable[11, 10] = "03-21";
            EasterTable[11, 11] = "03-22";

            EasterTable[12, 00] = "03-24";
            EasterTable[12, 01] = "03-31";
            EasterTable[12, 02] = "04-01";
            EasterTable[12, 03] = "04-02";
            EasterTable[12, 04] = "04-03";
            EasterTable[12, 05] = "04-04";
            EasterTable[12, 06] = "04-05";
            EasterTable[12, 07] = "04-06";
            EasterTable[12, 08] = "04-07";
            EasterTable[12, 09] = "04-08";
            EasterTable[12, 10] = "04-09";
            EasterTable[12, 11] = "04-10";

            EasterTable[13, 00] = "04-12";
            EasterTable[13, 01] = "04-18";
            EasterTable[13, 02] = "03-21";
            EasterTable[13, 03] = "03-22";
            EasterTable[13, 04] = "03-23";
            EasterTable[13, 05] = "03-24";
            EasterTable[13, 06] = "03-25";
            EasterTable[13, 07] = "03-26";
            EasterTable[13, 08] = "03-27";
            EasterTable[13, 09] = "03-28";
            EasterTable[13, 10] = "03-29";
            EasterTable[13, 11] = "03-30";

            EasterTable[14, 00] = "04-01";
            EasterTable[14, 01] = "04-08";
            EasterTable[14, 02] = "04-09";
            EasterTable[14, 03] = "04-10";
            EasterTable[14, 04] = "04-11";
            EasterTable[14, 05] = "04-12";
            EasterTable[14, 06] = "04-13";
            EasterTable[14, 07] = "04-14";
            EasterTable[14, 08] = "04-15";
            EasterTable[14, 09] = "04-16";
            EasterTable[14, 10] = "04-17";
            EasterTable[14, 11] = "04-17";

            EasterTable[15, 00] = "03-21";
            EasterTable[15, 01] = "03-28";
            EasterTable[15, 02] = "03-29";
            EasterTable[15, 03] = "03-30";
            EasterTable[15, 04] = "03-31";
            EasterTable[15, 05] = "04-01";
            EasterTable[15, 06] = "04-02";
            EasterTable[15, 07] = "04-03";
            EasterTable[15, 08] = "04-04";
            EasterTable[15, 09] = "04-05";
            EasterTable[15, 10] = "04-06";
            EasterTable[15, 11] = "04-07";

            EasterTable[16, 00] = "04-09";
            EasterTable[16, 01] = "04-16";
            EasterTable[16, 02] = "04-17";
            EasterTable[16, 03] = "04-17";
            EasterTable[16, 04] = "04-18";
            EasterTable[16, 05] = "03-21";
            EasterTable[16, 06] = "03-22";
            EasterTable[16, 07] = "03-23";
            EasterTable[16, 08] = "03-24";
            EasterTable[16, 09] = "03-25";
            EasterTable[16, 10] = "03-26";
            EasterTable[16, 11] = "03-27";

            EasterTable[17, 00] = "03-29";
            EasterTable[17, 01] = "04-05";
            EasterTable[17, 02] = "04-06";
            EasterTable[17, 03] = "04-07";
            EasterTable[17, 04] = "04-08";
            EasterTable[17, 05] = "04-09";
            EasterTable[17, 06] = "04-10";
            EasterTable[17, 07] = "04-11";
            EasterTable[17, 08] = "04-12";
            EasterTable[17, 09] = "04-13";
            EasterTable[17, 10] = "04-14";
            EasterTable[17, 11] = "04-15";

            EasterTable[18, 00] = "04-17";
            EasterTable[18, 01] = "03-25";
            EasterTable[18, 02] = "03-26";
            EasterTable[18, 03] = "03-27";
            EasterTable[18, 04] = "03-28";
            EasterTable[18, 05] = "03-29";
            EasterTable[18, 06] = "03-30";
            EasterTable[18, 07] = "03-31";
            EasterTable[18, 08] = "04-01";
            EasterTable[18, 09] = "04-02";
            EasterTable[18, 10] = "04-03";
            EasterTable[18, 11] = "04-04";

            return EasterTable;
        }

        public static int GetA(int month, int day)
        {
            if ((month == 3 && day == 26) || (month == 4 && day == 2) || (month == 4 && day == 9) || (month == 4 && day == 16)) return 0;
            else if ((month == 3 && day == 27) || (month == 4 && day == 3) || (month == 4 && day == 10) || (month == 4 && day == 17)) return 1;
            else if ((month == 3 && day == 21) || (month == 3 && day == 28) || (month == 4 && day == 4) || (month == 4 && day == 11) || (month == 4 && day == 18)) return 2;
            else if ((month == 3 && day == 22) || (month == 3 && day == 29) || (month == 4 && day == 5) || (month == 4 && day == 12)) return 3;
            else if ((month == 3 && day == 23) || (month == 3 && day == 30) || (month == 4 && day == 6) || (month == 4 && day == 13)) return 4;
            else if ((month == 3 && day == 24) || (month == 3 && day == 31) || (month == 4 && day == 7) || (month == 4 && day == 14)) return 5;
            else if ((month == 3 && day == 25) || (month == 4 && day == 1) || (month == 4 && day == 8) || (month == 4 && day == 15)) return 6;
            else throw new Exception("A value is not in table");
        }

        public static int GetB(int year)
        {
            var twodigityear = GetFirsttwodigit(year);
            if (twodigityear == 5 || twodigityear == 12 || twodigityear == 16 || twodigityear == 20 || twodigityear == 24 || twodigityear == 28 || twodigityear == 32 || twodigityear == 36 || twodigityear == 40) return 0;
            else if (twodigityear == 4 || twodigityear == 11 || twodigityear == 19 || twodigityear == 23 || twodigityear == 27 || twodigityear == 31 || twodigityear == 35 || twodigityear == 39 || (twodigityear >= 1583 && twodigityear <= 1599)) return 1;
            else if (twodigityear == 3 || twodigityear == 10) return 2;
            else if (twodigityear == 9 || twodigityear == 18 || twodigityear == 22 || twodigityear == 26 || twodigityear == 30 || twodigityear == 34 || twodigityear == 38) return 3;
            else if (twodigityear == 8 || (twodigityear >= 1500 && twodigityear <= 1582)) return 4;
            else if (twodigityear == 7 || twodigityear == 14 || twodigityear == 17 || twodigityear == 21 || twodigityear == 25 || twodigityear == 29 || twodigityear == 33 || twodigityear == 37) return 5;
            else if (twodigityear == 06 || twodigityear == 13) return 6;
            else throw new Exception("B value is not in table");
        }

        public static int GetC(int year)
        {
            var twodigityear = Getlasttwodigit(year);
            if (twodigityear == 0 || twodigityear == 6 || twodigityear == 17 || twodigityear == 23 || twodigityear == 28 || twodigityear == 34 || twodigityear == 45 || twodigityear == 51 || twodigityear == 56 || twodigityear == 62 || twodigityear == 73 || twodigityear == 79 || twodigityear == 84 || twodigityear == 90) return 0;
            else if (twodigityear == 1 || twodigityear == 7 || twodigityear == 12 || twodigityear == 18 || twodigityear == 29 || twodigityear == 35 || twodigityear == 40 || twodigityear == 46 || twodigityear == 57 || twodigityear == 63 || twodigityear == 68 || twodigityear == 74 || twodigityear == 85 || twodigityear == 91 || twodigityear == 96) return 1;
            else if (twodigityear == 2 || twodigityear == 13 || twodigityear == 19 || twodigityear == 24 || twodigityear == 30 || twodigityear == 41 || twodigityear == 47 || twodigityear == 52 || twodigityear == 58 || twodigityear == 69 || twodigityear == 75 || twodigityear == 80 || twodigityear == 86 || twodigityear == 97) return 2;
            else if (twodigityear == 3 || twodigityear == 8 || twodigityear == 14 || twodigityear == 25 || twodigityear == 31 || twodigityear == 36 || twodigityear == 42 || twodigityear == 53 || twodigityear == 59 || twodigityear == 64 || twodigityear == 70 || twodigityear == 81 || twodigityear == 87 || twodigityear == 92 || twodigityear == 98) return 3;
            else if (twodigityear == 9 || twodigityear == 15 || twodigityear == 20 || twodigityear == 26 || twodigityear == 37 || twodigityear == 43 || twodigityear == 48 || twodigityear == 54 || twodigityear == 65 || twodigityear == 71 || twodigityear == 76 || twodigityear == 82 || twodigityear == 93 || twodigityear == 99) return 4;
            else if (twodigityear == 4 || twodigityear == 10 || twodigityear == 21 || twodigityear == 27 || twodigityear == 32 || twodigityear == 38 || twodigityear == 49 || twodigityear == 55 || twodigityear == 60 || twodigityear == 66 || twodigityear == 77 || twodigityear == 83 || twodigityear == 88 || twodigityear == 94) return 5;
            else if (twodigityear == 5 || twodigityear == 11 || twodigityear == 16 || twodigityear == 22 || twodigityear == 33 || twodigityear == 39 || twodigityear == 44 || twodigityear == 50 || twodigityear == 61 || twodigityear == 67 || twodigityear == 72 || twodigityear == 78 || twodigityear == 89 || twodigityear == 95) return 6;
            else throw new Exception("C value is not in table");
        }

        public static int GetD(int sumabc)
        {
            if (sumabc == 6 || sumabc == 13) return 1;
            else if (sumabc == 5 || sumabc == 12) return 2;
            else if (sumabc == 4 || sumabc == 11 || sumabc == 18) return 3;
            else if (sumabc == 3 || sumabc == 10 || sumabc == 17) return 4;
            else if (sumabc == 2 || sumabc == 9 || sumabc == 16) return 5;
            else if (sumabc == 1 || sumabc == 8 || sumabc == 15) return 6;
            else if (sumabc == 0 || sumabc == 7 || sumabc == 14) return 7;
            else throw new Exception("D value is not in table");
        }

        static int GetFirsttwodigit(int year)
        {
            return GetDigits(year)[0] * 10 + GetDigits(year)[1];
        }

        static int Getlasttwodigit(int year)
        {
            return GetDigits(year)[2] * 10 + GetDigits(year)[3];
        }

        static int[] GetDigits(int year)
        {
            int dig4 = year % 10;
            year /= 10;
            int dig3 = year % 10;
            year /= 10;
            int dig2 = year % 10;

            int dig1 = year / 10;
            return new int[] { dig1, dig2, dig3, dig4 };
        }
    }

    internal class Holiday
    {
        public String Name { get; set; }
        public DateTime Date { get; set; }
    }
}
