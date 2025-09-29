namespace Olive.Entities.Data.IRepository.GenericRepository.Entities;
public interface IArchivableEntity : IEntity
{
    public bool IsArchived { get; set; }
}