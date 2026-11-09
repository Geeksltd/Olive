using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olive.Entities;
using System.Reflection;

namespace Olive.Entities.EF.Replication;

/// <summary>
/// Maps subscription-package entity types to the same table/schema names Olive replication uses at runtime.
/// </summary>
public static class ReplicationModelBuilderExtensions
{
    public static EntityTypeBuilder<T> ConfigureReplicationEntity<T>(
        this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<T>>? configure = null)
        where T : class
    {
        var type = typeof(T);
        var builder = modelBuilder.Entity<T>();
        builder.ToTable(TableNameAttribute.GetTableName(type), SchemaAttribute.GetSchema(type));
        configure?.Invoke(builder);
        return builder;
    }

    public static void ConfigureReplicationEntities(this ModelBuilder modelBuilder, params Type[] entityTypes)
    {
        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type).ToTable(
                TableNameAttribute.GetTableName(type),
                SchemaAttribute.GetSchema(type));
        }
    }

    public static void ConfigureReplicationEntities(this ModelBuilder modelBuilder, Assembly domainAssembly)
    {
        ConfigureReplicationEntities(modelBuilder, GetReplicationEntityTypes(domainAssembly));
    }

    public static Type[] GetReplicationEntityTypes(Assembly domainAssembly)
    {
        return domainAssembly.GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && typeof(IEntity).IsAssignableFrom(t)
                && t.GetCustomAttribute<SchemaAttribute>(inherit: false) != null)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToArray();
    }
}
