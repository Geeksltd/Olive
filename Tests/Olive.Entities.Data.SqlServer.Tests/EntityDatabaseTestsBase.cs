using NUnit.Framework;
using Olive.Tests;
using System;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    [TestFixture]
    public partial class EntityDatabaseTestsBase : TestsDatabaseBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            TempDatabaseName = new SqlServerManager().GetDatabaseName();

            base.InitDatabase();

            #region Create database for Person entity

            var server = new SqlServerManager();

            if (server.Exists(TempDatabaseName, DatabaseFilesPath.FullName)) server.Delete(TempDatabaseName);

            server.ClearConnectionPool();
            server.Delete(TempDatabaseName);

            foreach (var file in GetExecutableCreateDbScripts(TempDatabaseName))
            {
                try { server.Execute(file.Value, TempDatabaseName); }
                catch (Exception ex)
                { throw new Exception("Could not execute sql file '" + file.Key.FullName + "'", ex); }
            }

            server.ClearConnectionPool();

            #endregion
        }
    }
}