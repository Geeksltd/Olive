using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Audit;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Tests
{
    /// <summary>
    /// Set-based writes (query.Delete, query.Update, query.Claim, Database.Insert) through Olive's entity pipeline, the
    /// default, over mocked providers. The bypassEntityPipeline path is SQL and is covered by the database providers.
    /// </summary>
    [TestFixture]
    public class SetBasedWritesTests
    {
        public class Item : GuidEntity
        {
            public string Group { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public int? Score { get; set; }
            public byte Level { get; set; }
            public sbyte? Shift { get; set; }
            public DateTime? Since { get; set; }
            public Guid? OwnerId { get; set; }

            public bool CancelSave, CancelDelete, Invalid;

            /// <summary>How many times the association was read: reading one loads the record.</summary>
            public int OwnerReads;

            public Item Owner
            {
                get { OwnerReads++; return null; }
                set => OwnerId = value?.ID;
            }

            public string Label => Name + "!";

            public override Task Validate()
            {
                if (Invalid) throw new ValidationException("Invalid item.");
                return base.Validate();
            }

            protected override Task OnSaving(CancelEventArgs e)
            {
                e.Cancel = CancelSave;
                return base.OnSaving(e);
            }

            protected override Task OnDeleting(CancelEventArgs e)
            {
                e.Cancel = CancelDelete;
                return base.OnDeleting(e);
            }
        }

        [SoftDelete]
        public class Archived : GuidEntity
        {
            public string Group { get; set; }
        }

        Dictionary<Type, Mock<IDataProvider>> providers;
        Dictionary<Type, List<IEntity>> rows;
        Mock<IAudit> audit;
        IDatabase database;

        [SetUp]
        public void SetUp()
        {
            providers = new Dictionary<Type, Mock<IDataProvider>>();
            rows = new Dictionary<Type, List<IEntity>>();

            var config = new DatabaseConfig
            {
                Cache = new DatabaseConfig.CacheConfig { Enabled = false },
                Transaction = new DatabaseConfig.TransactionConfig { EnforceForSave = true }
            };

            var providerConfig = new Mock<IDatabaseProviderConfig>();
            providerConfig.Setup(x => x.Configuration).Returns(config);
            providerConfig.Setup(x => x.GetProvider(It.IsAny<Type>())).Returns((Type type) => GetProvider(type).Object);

            audit = new Mock<IAudit>();
            audit.Setup(x => x.LogInsert(It.IsAny<IEntity>())).Returns(Task.CompletedTask);
            audit.Setup(x => x.LogUpdate(It.IsAny<IEntity>())).Returns(Task.CompletedTask);
            audit.Setup(x => x.LogDelete(It.IsAny<IEntity>())).Returns(Task.CompletedTask);

            database = new Database(audit.Object, providerConfig.Object, new Cache(new InMemoryCacheProvider(), providerConfig.Object));

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton(database);
            var provider = services.BuildServiceProvider();
            Context.Initialize(provider, () => provider);
        }

        /// <summary>A provider over an in-memory table that matches criteria on equality, as the tests use them.</summary>
        Mock<IDataProvider> GetProvider(Type type)
        {
            if (providers.TryGetValue(type, out var result)) return result;

            result = providers[type] = new Mock<IDataProvider>();
            var records = rows[type] = new List<IEntity>();

            result.Setup(x => x.EntityType).Returns(type);
            result.Setup(x => x.SupportValidationBypassing()).Returns(true);

            result.Setup(x => x.GetList(It.IsAny<IDatabaseQuery>())).Returns((IDatabaseQuery query) =>
            {
                var matches = records.Where(r => !SoftDeleteAttribute.IsMarked((Entity)r) && Matches(r, query));
                if (query.TakeTop.HasValue) matches = matches.Take(query.TakeTop.Value);
                return Task.FromResult(matches.ToArray().AsEnumerable());
            });

            result.Setup(x => x.Get(It.IsAny<object>())).Returns((object id) =>
                Task.FromResult(records.Single(r => r.GetId().ToString() == id.ToString())));

            // A save replaces the stored row: an update saves a clone of the instance that was loaded.
            result.Setup(x => x.Save(It.IsAny<IEntity>())).Returns((IEntity entity) =>
            {
                records.RemoveAll(r => r.GetId().Equals(entity.GetId()));
                records.Add(entity);
                return Task.CompletedTask;
            });

            result.Setup(x => x.Delete(It.IsAny<IEntity>())).Returns((IEntity entity) =>
            {
                records.RemoveAll(r => r.GetId().Equals(entity.GetId()));
                return Task.CompletedTask;
            });

            return result;
        }

        static bool Matches(IEntity record, IDatabaseQuery query) =>
            query.Criteria.OfType<Criterion>().All(c =>
                Equals(record.GetType().GetProperty(c.PropertyName)?.GetValue(record)?.ToString(), c.Value?.ToString()));

        /// <summary>Stores the records as if loaded from the database, without raising anything.</summary>
        T[] Seed<T>(params T[] records) where T : Entity
        {
            GetProvider(typeof(T));
            foreach (var record in records)
            {
                Entity.Services.SetSaved(record);
                rows[typeof(T)].Add(record);
            }

            return records;
        }

        Item Stored(Guid id) => (Item)rows[typeof(Item)].Single(x => x.GetId().Equals(id));

        // ------------------------------------------------------------------ Delete

        [Test]
        public async Task Delete_deletes_each_matching_entity_through_the_pipeline()
        {
            Seed(new Item { Group = "a" }, new Item { Group = "a" }, new Item { Group = "a" }, new Item { Group = "b" });

            Assert.That(await database.Of<Item>().Where(x => x.Group == "a").Delete(), Is.EqualTo(3));

            Assert.That(rows[typeof(Item)].Cast<Item>().Select(x => x.Group), Is.EqualTo(new[] { "b" }));
            GetProvider(typeof(Item)).Verify(x => x.Delete(It.IsAny<IEntity>()), Times.Exactly(3));
            audit.Verify(x => x.LogDelete(It.IsAny<IEntity>()), Times.Exactly(3));
        }

        [Test]
        public async Task Delete_takes_at_most_Top()
        {
            Seed(Enumerable.Range(0, 5).Select(i => new Item { Group = "a" }).ToArray());

            Assert.That(await database.Of<Item>().Where(x => x.Group == "a").Top(2).Delete(), Is.EqualTo(2));
            Assert.That(rows[typeof(Item)], Has.Count.EqualTo(3));
        }

        [Test]
        public async Task Delete_counts_an_entity_whose_Deleting_handler_cancels_but_leaves_it()
        {
            var kept = new Item { Group = "a", CancelDelete = true };
            Seed(kept, new Item { Group = "a" });

            Assert.That(await database.Of<Item>().Where(x => x.Group == "a").Delete(), Is.EqualTo(2));

            Assert.That(rows[typeof(Item)], Is.EqualTo(new[] { kept }));
            GetProvider(typeof(Item)).Verify(x => x.Delete(It.IsAny<IEntity>()), Times.Once);
        }

        [Test]
        public async Task Delete_marks_a_soft_delete_entity_instead_of_removing_it()
        {
            var archived = Seed(new Archived { Group = "a" })[0];

            Assert.That(await database.Of<Archived>().Where(x => x.Group == "a").Delete(), Is.EqualTo(1));

            GetProvider(typeof(Archived)).Verify(x => x.Delete(It.IsAny<IEntity>()), Times.Never);
            GetProvider(typeof(Archived)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Once);
            Assert.That(SoftDeleteAttribute.IsMarked(archived), Is.True);
            Assert.That(await database.Of<Archived>().Count(), Is.Zero);
        }

        // ------------------------------------------------------------------ Update

        [Test]
        public async Task Update_makes_each_assignment_in_memory_as_the_statement_would()
        {
            var cutoff = new DateTime(2026, 1, 5);
            var (one, two, three) = (
                new Item { Group = "a", Count = 1, Score = null, Level = 250, Shift = null, Name = "", Since = new DateTime(2026, 1, 10) },
                new Item { Group = "a", Count = 2, Score = 5, Level = 3, Shift = -1, Name = "kept", Since = null },
                new Item { Group = "a", Count = 3, Score = 0, Level = 0, Shift = 0, Name = null, Since = new DateTime(2026, 1, 1) });
            Seed(one, two, three);

            var updated = await database.Of<Item>().Where(x => x.Group == "a").Update(set => set
                .Increment(x => x.Count, 10)
                .Increment(x => x.Level, (byte)5)
                .Increment(x => x.Shift, (sbyte?)2)
                .SetIfEmpty(x => x.Score, 7)
                .SetIfEmpty(x => x.Name, "filled")
                .SetIfEarlier(x => x.Since, cutoff));

            Assert.That(updated, Is.EqualTo(3));

            Assert.That(Stored(one.ID), Has.Property("Count").EqualTo(11).And.Property("Level").EqualTo(255)
                .And.Property("Shift").Null // null plus anything is null, as in SQL
                .And.Property("Score").EqualTo(7).And.Property("Name").EqualTo("filled").And.Property("Since").EqualTo(cutoff));

            Assert.That(Stored(two.ID), Has.Property("Count").EqualTo(12).And.Property("Level").EqualTo(8)
                .And.Property("Shift").EqualTo(1).And.Property("Score").EqualTo(5).And.Property("Name").EqualTo("kept")
                .And.Property("Since").EqualTo(cutoff));

            // Zero is a value, not empty; an earlier date stays.
            Assert.That(Stored(three.ID), Has.Property("Score").EqualTo(0).And.Property("Name").EqualTo("filled")
                .And.Property("Since").EqualTo(new DateTime(2026, 1, 1)));

            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Exactly(3));
            audit.Verify(x => x.LogUpdate(It.IsAny<IEntity>()), Times.Exactly(3));
        }

        [Test]
        public async Task SetIfEarlier_with_no_value_changes_nothing()
        {
            var since = new DateTime(2026, 1, 10);
            var item = Seed(new Item { Group = "a", Since = since })[0];

            await database.Of<Item>().Where(x => x.Group == "a").Update(set => set.SetIfEarlier(x => x.Since, null));

            Assert.That(Stored(item.ID).Since, Is.EqualTo(since));
        }

        [Test]
        public async Task Set_and_SetFrom_do_not_read_the_current_value()
        {
            var item = Seed(new Item { Group = "a", Name = "old", OwnerId = Guid.NewGuid() })[0];

            await database.Of<Item>().Where(x => x.Group == "a")
                .Update(set => set.Set(x => x.Owner, null).SetFrom(x => x.Name, x => x.Group));

            var stored = Stored(item.ID);
            Assert.That(stored.OwnerId, Is.Null);
            Assert.That(stored.Name, Is.EqualTo("a"));
            Assert.That(stored.OwnerReads, Is.Zero);
        }

        [Test]
        public async Task Update_counts_an_entity_whose_Saving_handler_cancels_but_leaves_it()
        {
            var kept = Seed(new Item { Group = "a", Count = 1, CancelSave = true }, new Item { Group = "a", Count = 1 });

            Assert.That(await database.Of<Item>().Where(x => x.Group == "a").Update(set => set.Increment(x => x.Count, 1)), Is.EqualTo(2));

            Assert.That(Stored(kept[0].ID).Count, Is.EqualTo(1));
            Assert.That(Stored(kept[1].ID).Count, Is.EqualTo(2));
            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Once);
        }

        [Test]
        public void Update_validates_each_entity()
        {
            Seed(new Item { Group = "a", Invalid = true });

            Assert.ThrowsAsync<ValidationException>(() =>
                database.Of<Item>().Where(x => x.Group == "a").Update(set => set.Set(x => x.Count, 1)));

            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Never);
        }

        [Test]
        public void Update_refuses_a_property_it_cannot_set()
        {
            Seed(new Item { Group = "a" });

            Assert.ThrowsAsync<ArgumentException>(() =>
                database.Of<Item>().Where(x => x.Group == "a").Update(set => set.Set(x => x.Label, "x")));
        }

        [Test]
        public void Update_needs_a_column()
        {
            Assert.ThrowsAsync<ArgumentException>(() => database.Of<Item>().Update(set => { }));
        }

        // ------------------------------------------------------------------ what the pipeline refuses

        [Test]
        public void The_pipeline_refuses_SkipLocked_and_paging()
        {
            Seed(new Item { Group = "a" });

            Assert.ThrowsAsync<ArgumentException>(() => database.Of<Item>().SkipLocked().Delete());
            Assert.ThrowsAsync<ArgumentException>(() => database.Of<Item>().SkipLocked().Update(set => set.Set(x => x.Count, 1)));

            var paged = database.Of<Item>();
            paged.PageSize = 10;
            Assert.ThrowsAsync<ArgumentException>(() => paged.Delete());

            Assert.That(rows[typeof(Item)], Has.Count.EqualTo(1));
            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Never);
        }

        [Test]
        public void Bypassing_on_a_provider_without_statements_throws_rather_than_using_the_pipeline()
        {
            Seed(new Item { Group = "a" });

            Assert.ThrowsAsync<NotSupportedException>(() => database.Of<Item>().Delete(bypassEntityPipeline: true));
            Assert.ThrowsAsync<NotSupportedException>(() =>
                database.Of<Item>().Update(set => set.Set(x => x.Count, 1), bypassEntityPipeline: true));
            Assert.ThrowsAsync<NotSupportedException>(() => database.Of<Item>().Claim(set => set.Set(x => x.Count, 1)));
            Assert.ThrowsAsync<NotSupportedException>(() => database.Insert(new[] { new Item() }, bypassEntityPipeline: true));

            GetProvider(typeof(Item)).Verify(x => x.Delete(It.IsAny<IEntity>()), Times.Never);
            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Never);
        }

        // ------------------------------------------------------------------ Insert

        [Test]
        public async Task Insert_saves_each_new_entity_of_any_type_through_the_pipeline()
        {
            await database.Insert(new IEntity[] { new Item { Group = "a" }, new Archived { Group = "a" }, new Item { Group = "b" } });

            Assert.That(rows[typeof(Item)], Has.Count.EqualTo(2));
            Assert.That(rows[typeof(Archived)], Has.Count.EqualTo(1));
            audit.Verify(x => x.LogInsert(It.IsAny<IEntity>()), Times.Exactly(3));
        }

        [Test]
        public void Insert_validates_and_takes_new_entities_only()
        {
            Assert.ThrowsAsync<ValidationException>(() => database.Insert(new[] { new Item { Invalid = true } }));

            var stored = Seed(new Item())[0];
            Assert.ThrowsAsync<ArgumentException>(() => database.Insert(new[] { stored }));

            GetProvider(typeof(Item)).Verify(x => x.Save(It.IsAny<IEntity>()), Times.Never);
        }
    }
}
