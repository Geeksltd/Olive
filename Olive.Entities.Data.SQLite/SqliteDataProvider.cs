using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data.SQLite
{
    public abstract class SqliteDataProvider<TTargetEntity> : SqliteDataProvider where TTargetEntity : IEntity
    {
        
    }
}
