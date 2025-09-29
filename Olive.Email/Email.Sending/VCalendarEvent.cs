using System;
using System.Text;

namespace Olive.Email
{
    public enum VCalendarPriority { Undefined = 0, High = 1, Normal = 5, Low = 9 }

    public enum VCalendarStatus { Tentative, Confirmed, Cancelled }

    public enum VCalendarTransparency { Opaque, Transparent }

    public enum VCalendarMethod { Publish, Request, Reply, Add, Cancel, Refresh, Counter, DeclineCounter }

    public enum VCalendarFrequency { Secondly, Minutely, Hourly, Daily, Weekly, Monthly, Yearly }

    public class VCalendarEvent
    {
        public string UniqueIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }
        public string OrganizerName { get; set; }
        public string[] Attendees { get; set; }
        public VCalendarPriority Priority { get; set; } = VCalendarPriority.Normal;
        public VCalendarStatus Status { get; set; } = VCalendarStatus.Confirmed;
        public VCalendarTransparency Transparency { get; set; } = VCalendarTransparency.Opaque;
        public string Categories { get; set; }
        public string Url { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime? Created { get; set; }
        public int? Sequence { get; set; }
        public VCalendarMethod Method { get; set; } = VCalendarMethod.Publish;
        public string TimeZone { get; set; }
        public bool AllDay { get; set; }
        public VCalendarRecurrence Recurrence { get; set; }
        public DateTime? RecurrenceEnd { get; set; }
        public string AlarmDescription { get; set; }
        public TimeSpan? AlarmTrigger { get; set; }
        public string ProductId { get; set; } = "-//Microsoft Corporation//Outlook 12.0 MIMEDIR//EN";

        public override string ToString() => ToVCalendarString();

        public string ToVCalendarString()
        {
            const string dateFormat = "yyyyMMddTHHmmssZ";
            const string dateOnlyFormat = "yyyyMMdd";

            var sb = new StringBuilder();

            // Calendar header
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine($"PRODID:{EscapeText(ProductId)}");
            sb.AppendLine("VERSION:1.0");

            if (Method != VCalendarMethod.Publish)
                sb.AppendLine($"METHOD:{Method.ToString().ToUpper()}");

            // Event
            sb.AppendLine("BEGIN:VEVENT");

            // Required fields
            sb.AppendLine($"UID:{EscapeText(UniqueIdentifier)}");

            if (AllDay)
            {
                sb.AppendLine($"DTSTART;VALUE=DATE:{Start.ToString(dateOnlyFormat)}");
                sb.AppendLine($"DTEND;VALUE=DATE:{End.ToString(dateOnlyFormat)}");
            }
            else
            {
                if (TimeZone.HasValue())
                {
                    sb.AppendLine($"DTSTART;TZID={TimeZone}:{Start:yyyyMMddTHHmmss}");
                    sb.AppendLine($"DTEND;TZID={TimeZone}:{End:yyyyMMddTHHmmss}");
                }
                else
                {
                    sb.AppendLine($"DTSTART:{Start.ToUniversalTime().ToString(dateFormat)}");
                    sb.AppendLine($"DTEND:{End.ToUniversalTime().ToString(dateFormat)}");
                }
            }

            sb.AppendLine($"SUMMARY:{EscapeText(Subject)}");

            // Optional fields
            if (Description.HasValue())
                sb.AppendLine($"DESCRIPTION:{EscapeText(Description)}");

            if (Location.HasValue())
                sb.AppendLine($"LOCATION:{EscapeText(Location)}");

            if (Organizer.HasValue())
            {
                if (OrganizerName.HasValue())
                    sb.AppendLine($"ORGANIZER;CN={EscapeText(OrganizerName)}:mailto:{Organizer}");
                else
                    sb.AppendLine($"ORGANIZER:mailto:{Organizer}");
            }

            if (Attendees != null && Attendees.Length > 0)
            {
                foreach (var attendee in Attendees)
                    sb.AppendLine($"ATTENDEE:mailto:{attendee}");
            }

            if (Priority != VCalendarPriority.Normal)
                sb.AppendLine($"PRIORITY:{(int)Priority}");

            if (Status != VCalendarStatus.Confirmed)
                sb.AppendLine($"STATUS:{Status.ToString().ToUpper()}");

            if (Transparency != VCalendarTransparency.Opaque)
                sb.AppendLine($"TRANSP:{Transparency.ToString().ToUpper()}");

            if (Categories.HasValue())
                sb.AppendLine($"CATEGORIES:{EscapeText(Categories)}");

            if (Url.HasValue())
                sb.AppendLine($"URL:{Url}");

            if (Created.HasValue)
                sb.AppendLine($"CREATED:{Created.Value.ToUniversalTime().ToString(dateFormat)}");

            if (LastModified.HasValue)
                sb.AppendLine($"LAST-MODIFIED:{LastModified.Value.ToUniversalTime().ToString(dateFormat)}");
            else
                sb.AppendLine($"LAST-MODIFIED:{DateTime.UtcNow.ToString(dateFormat)}");

            if (Sequence.HasValue)
                sb.AppendLine($"SEQUENCE:{Sequence.Value}");

            // Recurrence
            if (Recurrence != null)
            {
                sb.AppendLine($"RRULE:{Recurrence}");

                if (RecurrenceEnd.HasValue)
                    sb.AppendLine($"UNTIL:{RecurrenceEnd.Value.ToUniversalTime().ToString(dateFormat)}");
            }

            // Alarm
            if (AlarmTrigger.HasValue)
            {
                sb.AppendLine("BEGIN:VALARM");
                sb.AppendLine("ACTION:DISPLAY");
                sb.AppendLine($"DESCRIPTION:{EscapeText(AlarmDescription ?? Subject)}");

                var trigger = AlarmTrigger.Value;

                var triggerStr = trigger.TotalSeconds < 0 ?
                    $"-PT{Math.Abs(trigger.TotalMinutes):F0}M" :
                    $"PT{trigger.TotalMinutes:F0}M";

                sb.AppendLine($"TRIGGER:{triggerStr}");
                sb.AppendLine("END:VALARM");
            }

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        static string EscapeText(string text)
        {
            return text.OrEmpty().Replace("\\", "\\\\")
                       .Replace("\r", "")
                       .Replace("\n", "\\n")
                       .Replace(",", "\\,")
                       .Replace(";", "\\;");
        }
    }

    public class VCalendarRecurrence
    {
        public VCalendarFrequency Frequency { get; set; }
        public int? Interval { get; set; }
        public int? Count { get; set; }
        public DateTime? Until { get; set; }
        public DayOfWeek[] ByDay { get; set; }
        public int[] ByMonthDay { get; set; }
        public int[] ByMonth { get; set; }

        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string>();

            parts.Add($"FREQ={Frequency.ToString().ToUpper()}");

            if (Interval.HasValue && Interval.Value > 1)
                parts.Add($"INTERVAL={Interval.Value}");

            if (Count.HasValue)
                parts.Add($"COUNT={Count.Value}");
            else if (Until.HasValue)
                parts.Add($"UNTIL={Until.Value.ToUniversalTime():yyyyMMddTHHmmssZ}");

            if (ByDay != null && ByDay.Length > 0)
            {
                var days = Array.ConvertAll(ByDay, d => d.ToString().Substring(0, 2).ToUpper()).ToString(",");
                parts.Add($"BYDAY={days}");
            }

            if (ByMonthDay != null && ByMonthDay.Length > 0)
                parts.Add("BYMONTHDAY=" + ByMonthDay.ToString(","));

            if (ByMonth != null && ByMonth.Length > 0)
                parts.Add($"BYMONTH=" + ByMonth.ToString(","));

            return parts.ToString(";");
        }
    }
}