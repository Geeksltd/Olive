namespace Domain
{
    using System.Linq.Expressions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    
    /// <summary>Provides the ability to inject filter business logic into database queries.</summary>
    public static partial class DatabaseQueryExtensions
    {
        static IDatabase Database => Context.Current.Database();
    }
}