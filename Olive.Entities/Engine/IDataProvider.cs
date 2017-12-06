namespace Olive.Entities
{
    public interface IDataProvider
    {
        Task<IEntity> Get(object objectID);
        Task Save(IEntity record);
        Task Delete(IEntity record);

        Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query);

        /// <summary>
        /// Returns a direct database criterion used to eager load associated objects.
        /// </summary>
        DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery query, PropertyInfo association);

        IDataAccess Access { get; }

        Task<int> Count(IDatabaseQuery query);

        Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName);

        Type EntityType { get; }

        string MapColumn(string propertyName);
        string MapSubquery(string path);

        /// <summary>
        /// Reads the many to many relation and returns the IDs of the associated objects.
        /// </summary>
        Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property);

        IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated);

        Task<int> ExecuteNonQuery(string command);
        Task<object> ExecuteScalar(string command);

        bool SupportValidationBypassing();

        Task BulkInsert(IEntity[] entities, int batchSize);
        Task BulkUpdate(IEntity[] entities, int batchSize);

        string ConnectionString { get; set; }
        string ConnectionStringKey { get; set; }
    }
}