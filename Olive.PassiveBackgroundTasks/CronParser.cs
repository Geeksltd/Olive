using Cronos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.PassiveBackgroundTasks
{
    class CronParser
    {
        internal static int Minutes(string cronExpression)
        {
            var expression = CronExpression.Parse(cronExpression);
            var now = DateTime.UtcNow;
            var firstTwo = expression.GetOccurrences(now, now.AddYears(100), fromInclusive: true, toInclusive: false).Skip(1).Take(2);
            var first = firstTwo.First();
            var second = firstTwo.Last();

            return (int)second.Subtract(first).TotalMinutes;
        }
    }
}
