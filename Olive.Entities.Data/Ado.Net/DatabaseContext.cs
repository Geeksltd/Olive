using System;

namespace Olive.Entities.Data
{
    public class DatabaseContext : IDisposable
    {
        #region ConnectionString
        /// <summary>
        /// Gets or sets the ConnectionString of this DatabaseContext.
        /// </summary>
        public string ConnectionString { get; set; }

        public int? CommandTimeout { get; set; }

        #endregion

        DatabaseContext Parent;

        public DatabaseContext(string connectionString)
        {
            ConnectionString = connectionString;

            if (Current != null) Parent = Current;

            Current = this;
        }

        public static DatabaseContext Current
        {
            get => CallContext<DatabaseContext>.GetData(nameof(Current));
            set => CallContext<DatabaseContext>.SetData(nameof(Current), value);
        }

        public void Dispose() => Current = Parent;
    }
}
