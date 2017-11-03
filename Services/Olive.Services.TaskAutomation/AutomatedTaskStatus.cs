using Olive.Entities;

namespace Olive.Services.TaskAutomation
{
    /// <summary>
    /// Represents an instance of Automated Task Status entity type.
    /// </summary>
    [TransientEntity]
    public partial class AutomatedTaskStatus //: Entity
    {
        /* -------------------------- Properties -------------------------*/

        #region Name Property

        /// <summary>
        /// Gets or sets the value of Name on this Automated Task Status instance.
        /// </summary>
        public string Name { get; set; }

        #endregion

        /* -------------------------- Methods ----------------------------*/

        /// <summary>
        /// Returns a textual representation of this Automated Task Status.
        /// </summary>
        /// <returns>A string value that represents this Automated Task Status instance.</returns>
        public override string ToString() => Name.Or(string.Empty);
    }
}