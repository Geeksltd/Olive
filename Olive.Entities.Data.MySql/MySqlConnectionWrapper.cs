using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.Data
{
    public class MySqlConnectionWrapper : DbConnection
    {
        readonly MySqlConnection RealConnection = new MySqlConnection();

        public override string ConnectionString
        {
            get => RealConnection.ConnectionString;
            set => RealConnection.ConnectionString = value;
        }

        public override string Database => RealConnection.Database;

        public override string DataSource => RealConnection.DataSource;

        public override string ServerVersion => RealConnection.ServerVersion;

        public override ConnectionState State => RealConnection.State;

        public override void ChangeDatabase(string databaseName) => RealConnection.ChangeDatabase(databaseName);

        public override void Close() => RealConnection.Close();

        public override void Open() => RealConnection.Open();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
            RealConnection.BeginTransaction(isolationLevel);

        protected override DbCommand CreateDbCommand() => new MySqlCommand { Connection = RealConnection };
    }
}
