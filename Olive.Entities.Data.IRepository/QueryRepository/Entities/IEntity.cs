namespace Olive.Entities.Data.IRepository.GenericRepository.Entities;
/// <summary>
///   <br />
/// </summary>
public interface IEntity
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    Guid Id { get; set; }

    /// <summary>Gets or sets the date created.</summary>
    /// <value>The date created.</value>
    DateTime DateCreated { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    /// <value>
    /// The created by.
    /// </value>
    Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date modified.
    /// </summary>
    /// <value>
    /// The date modified.
    /// </value>
    DateTime? DateModified { get; set; }

    /// <summary>
    /// Gets or sets the modified by.
    /// </summary>
    /// <value>
    /// The modified by.
    /// </value>
    Guid? ModifiedById { get; set; }
}
