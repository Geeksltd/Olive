namespace AppData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    using Olive.Entities.Data.SqlServer.Tests;

    /// <summary>Provides data-access facilities for Groups.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [EscapeGCop("Auto generated code.")]
    public class GroupDataProvider : SqlDataProvider<Group>
    {
        public GroupDataProvider(ICache cache) : base(cache) { }
        
        public override Type EntityType => typeof(Group);
        
        #region SQL Commands
        
        /// <summary>Gets a SQL command text to query a single Group record</summary>
        
        /// <summary>Gets the list of fields to use for loading Groups.</summary>
        public override string GetFields()
        {
            return @"G.ID AS Groups_Id,
            G.Name AS Groups_Name,
            G.DateCreated AS Groups_DateCreated";
        }
        
        /// <summary>Provides the data source expression for querying Group records.</summary>
        public override string GetTables(string prefix = null)
        {
            return $@"Groups AS {prefix}G";
        }
        
        /// <summary>Gets a SQL command text to insert a record into Groups table.</summary>
        const string INSERT_COMMAND = @"INSERT INTO Groups
        (Id, Name, DateCreated)
        VALUES
        (@Id, @Name, @DateCreated)";
        
        /// <summary>Gets a SQL command text to update a record in Groups table.</summary>
        const string UPDATE_COMMAND = @"UPDATE Groups SET
        Id = @Id,
        Name = @Name,
        DateCreated = @DateCreated
        OUTPUT INSERTED.ID
        WHERE ID = @OriginalId";
        
        /// <summary>Gets a SQL command text to delete a record from Groups table.</summary>
        const string DELETE_COMMAND = @"DELETE FROM Groups WHERE ID = @Id";
        
        #endregion
        
        /// <summary>Gets the database column name for a specified Group property.</summary>
        public override string MapColumn(string propertyName) => $"G.[{propertyName}]";
        
        /// <summary>
        /// Lazy-loads the data for the specified many-to-many relation on the specified Group instance from the database.<para/>
        /// </summary>
        public override Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
        }
        
        /// <summary>Extracts the Group instance from the current record of the specified data reader.</summary>
        public override IEntity Parse(IDataReader reader)
        {
            var result = new Group();
            FillData(reader, result);
            Entity.Services.SetSaved(result, reader.GetGuid(0));
            return result;
        }
        
        /// <summary>Loads the data from the specified data reader on the specified Group instance.</summary>
        internal void FillData(IDataReader reader, Group entity)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            
            entity.Name = values[Fields.Name] as string;
            entity.DateCreated = (DateTime)values[Fields.DateCreated];
        }
        
        /// <summary>Saves the specified Group instance in the database.</summary>
        public override async Task Save(IEntity record)
        {
            var item = record as Group;
            
            if (record.IsNew)
                await Insert(item);
            else
                await Update(item);
        }
        
        /// <summary>Inserts the specified new Group instance into the database.</summary>
        async Task Insert(Group item)
        {
            await ExecuteScalar(INSERT_COMMAND, CommandType.Text,
            CreateParameters(item));
        }
        
        /// <summary>Bulk inserts a number of specified Groups into the database.</summary>
        public override async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            var commands = new List<KeyValuePair<string, IDataParameter[]>>();
            
            foreach (var item in entities.Cast<Group>())
            {
                commands.Add(INSERT_COMMAND, CreateParameters(item));
            }
            
            await Access.ExecuteBulkNonQueries(CommandType.Text, commands);
        }
        
        /// <summary>Updates the specified existing Group instance in the database.</summary>
        async Task Update(Group item)
        {
            if ((await ExecuteScalar(UPDATE_COMMAND, CommandType.Text, CreateParameters(item))).ToStringOrEmpty().IsEmpty())
            {
                Cache.Remove(item);
                throw new ConcurrencyException($"Failed to update the 'Groups' table. There is no row with the ID of {item.ID}.");
            }
        }
        
        /// <summary>Creates parameters for Inserting or Updating Group records</summary>
        IDataParameter[] CreateParameters(Group item)
        {
            var result = new List<IDataParameter>();
            
            result.Add(CreateParameter("OriginalId", item.OriginalId));
            result.Add(CreateParameter("Id", item.GetId()));
            result.Add(CreateParameter("Name", item.Name));
            result.Add(CreateParameter("DateCreated", item.DateCreated, DbType.DateTime2));
            
            return result.ToArray();
        }
        
        /// <summary>Deletes the specified Group instance from the database.</summary>
        public override async Task Delete(IEntity record)
        {
            await ExecuteNonQuery(DELETE_COMMAND, System.Data.CommandType.Text, CreateParameter("Id", record.GetId()));
        }
        
        static class Fields
        {
            public const int Name = 1;
            public const int DateCreated = 2;
        }
    }
}