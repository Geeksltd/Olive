using Olive.Entities;

namespace Olive.Email
{
    /// <summary>
    /// Represents an instance of Email template entity type.
    /// </summary>
    public partial interface IEmailTemplate : IEntity
    {
        /// <summary>
        /// Gets or sets the value of Body on this Email template instance.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Gets or sets the value of Key on this Email template instance.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Gets or sets the value of MandatoryPlaceholders on this Email template instance.
        /// </summary>
        string MandatoryPlaceholders { get; set; }

        /// <summary>
        /// Gets or sets the value of Subject on this Email template instance.
        /// </summary>
        string Subject { get; set; }
    }
}