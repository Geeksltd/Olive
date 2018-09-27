namespace AppData
{
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    using Olive.Entities.Data.MySql.Tests;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>Provides data-access facilities for People.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [EscapeGCop("Auto generated code.")]
    public class PersonDataProvider : MySqlDataProvider<Person>
    {
        public override Type EntityType => typeof(Person);
        public PersonDataProvider(ICache cache) : base(cache)
        {
        }
        #region SQL Commands

        /// <summary>Gets a SQL command text to query a single Person record</summary>

        /// <summary>Gets the list of fields to use for loading People.</summary>
        public override string GetFields()
        {
            return @"P.`ID` AS `People_Id`,
            P.`FirstName` AS `People_FirstName`,
            P.`LastName` AS `People_LastName`";
        }

        /// <summary>Provides the data source expression for querying Person records.</summary>
        public override string GetTables() => @"`People` AS P";

        /// <summary>Gets a SQL command text to insert a record into People table.</summary>
        const string INSERT_COMMAND = @"INSERT INTO `People`
        (`Id`, `FirstName`, `LastName`)
        VALUES
        (@Id, @FirstName, @LastName)";

        /// <summary>Gets a SQL command text to update a record in People table.</summary>
        const string UPDATE_COMMAND = @"SELECT `ID` FROM `People` WHERE `ID` = @OriginalId;
        UPDATE `People` SET
        `Id` = @Id,
        `FirstName` = @FirstName,
        `LastName` = @LastName
        WHERE `ID` = @OriginalId";

        /// <summary>Gets a SQL command text to delete a record from People table.</summary>
        const string DELETE_COMMAND = @"DELETE FROM `People` WHERE `ID` = @Id";



        #endregion

        /// <summary>Gets the database column name for a specified Person property.</summary>
        public override string MapColumn(string propertyName) => $"P.`{propertyName}`";

        /// <summary>
        /// Lazy-loads the data for the specified many-to-many relation on the specified Person instance from the database.<para/>
        /// </summary>
        public override Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
        }

        /// <summary>Extracts the Person instance from the current record of the specified data reader.</summary>
        public override IEntity Parse(IDataReader reader)
        {
            var result = new Person();
            FillData(reader, result);
            Entity.Services.SetSaved(result, reader.GetGuid(0));
            return result;
        }

        /// <summary>Loads the data from the specified data reader on the specified Person instance.</summary>
        internal void FillData(IDataReader reader, Person entity)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);

            entity.FirstName = values[Fields.FirstName] as string;
            entity.LastName = values[Fields.LastName] as string;
        }

        /// <summary>Saves the specified Person instance in the database.</summary>
        public override async Task Save(IEntity record)
        {
            var item = record as Person;

            if (record.IsNew) await Insert(item);
            else await Update(item);
        }

        /// <summary>Inserts the specified new Person instance into the database.</summary>
        async Task Insert(Person item)
        {
            await ExecuteScalar(INSERT_COMMAND, CommandType.Text,
            CreateParameters(item));
        }

        /// <summary>Bulk inserts a number of specified People into the database.</summary>
        public override async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            var commands = new List<KeyValuePair<string, IDataParameter[]>>();

            foreach (var item in entities.Cast<Person>())
                commands.Add(INSERT_COMMAND, CreateParameters(item));


            await Access.ExecuteBulkNonQueries(CommandType.Text, commands);
        }

        /// <summary>Updates the specified existing Person instance in the database.</summary>
        async Task Update(Person item)
        {
            if ((await ExecuteScalar(UPDATE_COMMAND, CommandType.Text, CreateParameters(item))).ToStringOrEmpty().IsEmpty())
            {
                Cache.Remove(item);
                throw new ConcurrencyException($"Failed to update the 'People' table. There is no row with the ID of {item.ID}.");
            }
        }

        /// <summary>Creates parameters for Inserting or Updating Person records</summary>
        IDataParameter[] CreateParameters(Person item)
        {
            var result = new List<IDataParameter>();

            result.Add(CreateParameter("OriginalId", item.OriginalId));
            result.Add(CreateParameter("Id", item.GetId()));
            result.Add(CreateParameter("FirstName", item.FirstName));
            result.Add(CreateParameter("LastName", item.LastName));

            return result.ToArray();
        }

        /// <summary>Deletes the specified Person instance from the database.</summary>
        public override async Task Delete(IEntity record)
        {
            await ExecuteNonQuery(DELETE_COMMAND, System.Data.CommandType.Text, CreateParameter("Id", record.GetId()));
        }

        static class Fields
        {
            public const int FirstName = 1;
            public const int LastName = 2;
        }
    }
}