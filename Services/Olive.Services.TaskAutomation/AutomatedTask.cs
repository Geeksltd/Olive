using System;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Services.TaskAutomation
{
    /// <summary>
    /// Represents an instance of Automated task entity type.
    /// </summary>
    [TransientEntity]
    public partial class AutomatedTask : GuidEntity
    {
        const int DEFAULT_DELAY_IN_SECONDS = 30, NAME_MAX_LENGTH = 200;

        /* -------------------------- Constructor -----------------------*/

        /// <summary>
        /// Initializes a new instance of the AutomatedTask class.
        /// </summary>
        public AutomatedTask()
        {
            RecordFailure = true;

            Delay = TimeSpan.FromSeconds(DEFAULT_DELAY_IN_SECONDS);
        }

        /* -------------------------- Properties -------------------------*/

        #region Current execution duration Property

        /// <summary>
        /// Gets the CurrentExecutionDuration property.
        /// </summary>
        public string CurrentExecutionDuration => CurrentStartTime == null ? "" : "Since " + CurrentStartTime.Value.ToTimeDifferenceString();

        #endregion

        #region Current start time Property

        /// <summary>
        /// Gets or sets the value of CurrentStartTime on this Automated task instance.
        /// </summary>
        public DateTime? CurrentStartTime { get; set; }

        #endregion

        #region Last run duration Property

        /// <summary>
        /// Gets the LastRunDuration property.
        /// </summary>
        public TimeSpan? LastRunDuration
        {
            get
            {
                if (LastRunStart == null || LastRunEnd == null) return null;
                else return LastRunEnd.Value.Subtract(LastRunStart.Value);
            }
        }

        #endregion

        #region Last run end Property

        /// <summary>
        /// Gets or sets the value of LastRunEnd on this Automated task instance.
        /// </summary>
        public DateTime? LastRunEnd { get; set; }

        #endregion

        #region Last run start Property

        /// <summary>
        /// Gets or sets the value of LastRunStart on this Automated task instance.
        /// </summary>
        public DateTime? LastRunStart { get; set; }

        #endregion

        #region Name Property

        /// <summary>
        /// Gets or sets the value of Name on this Automated task instance.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Next try Property

        /// <summary>
        /// Gets or sets the value of NextTry on this Automated task instance.
        /// </summary>
        public DateTime? NextTry { get; set; }

        #endregion

        #region Record failure Property

        /// <summary>
        /// Gets or sets a value indicating whether this Automated task instance Record failure.
        /// </summary>
        public bool RecordFailure { get; set; }

        #endregion

        #region Record success Property

        /// <summary>
        /// Gets or sets a value indicating whether this Automated task instance Record success.
        /// </summary>
        public bool RecordSuccess { get; set; }

        #endregion

        #region Delay
        /// <summary>
        /// Gets or sets the Delay of this AutomatedTask.
        /// </summary>
        public TimeSpan Delay { get; set; }
        #endregion

        #region SyncGroup
        /// <summary>
        /// Gets or sets the SyncGroup of this AutomatedTask.
        /// </summary>
        public AsyncLock AsyncGroup { private get; set; }
        #endregion

        /* -------------------------- Methods ----------------------------*/

        /// <summary>
        /// Returns a textual representation of this Automated task.
        /// </summary>
        /// <returns>A string value that represents this Automated task instance.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Validates the data for the properties of this Automated task.
        /// It throws a ValidationException if an error is detected.
        /// </summary>
        protected override Task ValidateProperties()
        {
            // Validate Name property:
            if (Name.IsEmpty())
                throw new ValidationException("Name cannot be empty.");

            if (Name.Length > NAME_MAX_LENGTH)
                throw new ValidationException("Name field allows a maximum of 200 characters. You have provided {0} characters which exceeds this limit.", Name.Length);

            return Task.CompletedTask;
        }
    }
}