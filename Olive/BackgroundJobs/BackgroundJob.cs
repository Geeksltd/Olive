using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    public class BackgroundJob
    {
        public BackgroundJob(string name, Expression<Func<Task>> action, string scheduleCron)
        {
            Name = name;
            Action = action;
            ScheduleCron = scheduleCron;
        }

        /// <summary>
        /// The name of this background job.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A cron expression is a string consisting of six or seven subexpressions (fields) that describe individual details of the schedule.
        /// These fields, separated by white space, can contain any of the allowed values with various combinations of the allowed characters for that field.
        /// </summary>
        public string ScheduleCron { get; set; }
        public int TimeoutInMinutes { get; set; } = 5;

        /// <summary>
        /// The action to run in the schedule.
        /// </summary>
        public Expression<Func<Task>> Action { get; set; }

        public string SyncGroup { get; set; } = "default";
    }
}
