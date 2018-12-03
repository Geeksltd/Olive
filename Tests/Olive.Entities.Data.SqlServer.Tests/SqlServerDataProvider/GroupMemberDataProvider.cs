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

    /// <summary>Provides data-access facilities for Group members.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [EscapeGCop("Auto generated code.")]
    public class GroupMemberDataProvider : SqlDataProvider<GroupMember>
    {
        public GroupMemberDataProvider(ICache cache) : base(cache) { }
        
        public override Type EntityType => typeof(GroupMember);
        
        #region SQL Commands
        
        /// <summary>Gets a SQL command text to query a single Group member record</summary>
        
        /// <summary>Gets the list of fields to use for loading Group members.</summary>
        public override string GetFields()
        {
            return @"G.ID AS GroupMembers_Id,
            G.[Group] AS GroupMembers_Group,
            G.Person AS GroupMembers_Person,
            G.DateRegistered AS GroupMembers_DateRegistered";
        }
        
        /// <summary>Provides the data source expression for querying Group member records.</summary>
        public override string GetTables(string prefix = null)
        {
            return $@"GroupMembers AS {prefix}G";
        }
        
        /// <summary>Gets a SQL command text to insert a record into GroupMembers table.</summary>
        const string INSERT_COMMAND = @"INSERT INTO GroupMembers
        (Id, [Group], Person, DateRegistered)
        VALUES
        (@Id, @Group, @Person, @DateRegistered)";
        
        /// <summary>Gets a SQL command text to update a record in GroupMembers table.</summary>
        const string UPDATE_COMMAND = @"UPDATE GroupMembers SET
        Id = @Id,
        [Group] = @Group,
        Person = @Person,
        DateRegistered = @DateRegistered
        OUTPUT INSERTED.ID
        WHERE ID = @OriginalId";
        
        /// <summary>Gets a SQL command text to delete a record from GroupMembers table.</summary>
        const string DELETE_COMMAND = @"DELETE FROM GroupMembers WHERE ID = @Id";
        
        #endregion
        
        /// <summary>Gets the database column name for a specified GroupMember property.</summary>
        public override string MapColumn(string propertyName) => $"G.[{propertyName}]";
        
        public override string MapSubquery(string path, string parent)
        {
            if (path == "Group.*")
            {
                var targetGroups = $"[{parent}.Group_Group]";
                parent = $"[{parent.Or("G")}]";
                
                return $@"SELECT {targetGroups}.ID
                FROM Groups AS {targetGroups}
                WHERE {targetGroups}.ID = {parent}.[Group]";
            }
            
            if (path == "Person.*")
            {
                var targetPeople = $"[{parent}.Person_Person]";
                parent = $"[{parent.Or("G")}]";
                
                return $@"SELECT {targetPeople}.ID
                FROM People AS {targetPeople}
                WHERE {targetPeople}.ID = {parent}.Person";
            }
            
            return base.MapSubquery(path, parent);
        }
        
        /// <summary>
        /// Lazy-loads the data for the specified many-to-many relation on the specified Group member instance from the database.<para/>
        /// </summary>
        public override Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
        }
        
        /// <summary>Extracts the Group member instance from the current record of the specified data reader.</summary>
        public override IEntity Parse(IDataReader reader)
        {
            var result = new GroupMember();
            FillData(reader, result);
            Entity.Services.SetSaved(result, reader.GetGuid(0));
            return result;
        }
        
        /// <summary>Loads the data from the specified data reader on the specified Group member instance.</summary>
        internal void FillData(IDataReader reader, GroupMember entity)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            
            if (values[Fields.Group] != DBNull.Value) entity.GroupId = (Guid)values[Fields.Group];
            
            if (values[Fields.Person] != DBNull.Value) entity.PersonId = (Guid)values[Fields.Person];
            entity.DateRegistered = (DateTime)values[Fields.DateRegistered];
        }
        
        /// <summary>Saves the specified Group member instance in the database.</summary>
        public override async Task Save(IEntity record)
        {
            var item = record as GroupMember;
            
            if (record.IsNew)
                await Insert(item);
            else
                await Update(item);
        }
        
        /// <summary>Inserts the specified new Group member instance into the database.</summary>
        async Task Insert(GroupMember item)
        {
            await ExecuteScalar(INSERT_COMMAND, CommandType.Text,
            CreateParameters(item));
        }
        
        /// <summary>Bulk inserts a number of specified Group members into the database.</summary>
        public override async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            var commands = new List<KeyValuePair<string, IDataParameter[]>>();
            
            foreach (var item in entities.Cast<GroupMember>())
            {
                commands.Add(INSERT_COMMAND, CreateParameters(item));
            }
            
            await Access.ExecuteBulkNonQueries(CommandType.Text, commands);
        }
        
        /// <summary>Updates the specified existing Group member instance in the database.</summary>
        async Task Update(GroupMember item)
        {
            if ((await ExecuteScalar(UPDATE_COMMAND, CommandType.Text, CreateParameters(item))).ToStringOrEmpty().IsEmpty())
            {
                Cache.Remove(item);
                throw new ConcurrencyException($"Failed to update the 'GroupMembers' table. There is no row with the ID of {item.ID}.");
            }
        }
        
        /// <summary>Creates parameters for Inserting or Updating Group member records</summary>
        IDataParameter[] CreateParameters(GroupMember item)
        {
            var result = new List<IDataParameter>();
            
            result.Add(CreateParameter("OriginalId", item.OriginalId));
            result.Add(CreateParameter("Id", item.GetId()));
            result.Add(CreateParameter("Group", item.GroupId));
            result.Add(CreateParameter("Person", item.PersonId));
            result.Add(CreateParameter("DateRegistered", item.DateRegistered, DbType.DateTime2));
            
            return result.ToArray();
        }
        
        /// <summary>Deletes the specified Group member instance from the database.</summary>
        public override async Task Delete(IEntity record)
        {
            await ExecuteNonQuery(DELETE_COMMAND, System.Data.CommandType.Text, CreateParameter("Id", record.GetId()));
        }
        
        static class Fields
        {
            public const int Group = 1;
            public const int Person = 2;
            public const int DateRegistered = 3;
        }
    }
}