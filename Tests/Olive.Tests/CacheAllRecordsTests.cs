using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Tests
{
    [TestFixture]
    public class CacheAllRecordsTests
    {
        [CacheAllRecords]
        public class Status : StringEntity { }

        [CacheAllRecords, SoftDelete]
        public class Tag : StringEntity { }

        public class Plain : StringEntity { }

        /// <summary>The attribute is not inherited, so this type loads records one at a time.</summary>
        public class SubStatus : Status { }

        [CacheAllRecords]
        public class Region : GuidEntity { }

        /// <summary>Reference data whose ToString() reads another record of the same type.</summary>
        [CacheAllRecords]
        public class Node : StringEntity
        {
            StringCachedReference<Node> cachedParent = new StringCachedReference<Node>();

            public string ParentId { get; set; }

            public Node Parent => cachedParent.GetOrDefault(ParentId);

            public override string ToString() => Parent == null ? ID : Parent + " > " + ID;
        }

        /// <summary>
        /// The shape M# generates for a CacheAllRecords type with instance accessors that declare their IDs.
        /// </summary>
        [CacheAllRecords]
        public class Stage : StringEntity
        {
            static readonly StringCachedReference<Stage> draft = new StringCachedReference<Stage>();

            public string Name { get; set; }

            public override string ToString() => Name;

            public static Stage Draft => draft.Get(DraftId, () => ParseById(DraftId));

            public const string DraftId = "draft";

            public static async Task<Stage> Parse(string text)
            {
                if (text.IsEmpty()) throw new ArgumentNullException(nameof(text));

                return (await Database.TryLoadAllRecords<Stage>())?.FirstOrDefault(s => s.Name == text) ??
                    await Database.FirstOrDefault<Stage>(s => s.Name == text);
            }

            public static Task<Stage[]> LoadAll() => Database.GetList<Stage>();

            public static Task<Stage> ParseById(string id) => Database.FindById<Stage>(id);
        }

        /// <summary>
        /// The shape M# generates for the instance accessors of a soft deleted CacheAllRecords type.
        /// </summary>
        [CacheAllRecords, SoftDelete]
        public class Phase : StringEntity
        {
            static readonly StringCachedReference<Phase> retired = new StringCachedReference<Phase>();

            public static Phase Retired => retired.Get(RetiredId, () => ParseById(RetiredId)) is Phase result &&
                !result.IsMarkedSoftDeleted ? result : null;

            public const string RetiredId = "retired";

            public static Task<Phase> ParseById(string id) => Database.FindById<Phase>(id);
        }

        public class Owner : GuidEntity
        {
            StringCachedReference<Status> cachedStatus = new StringCachedReference<Status>();

            public string StatusId { get; set; }

            public Status Status => cachedStatus.GetOrDefault(StatusId);
        }

        [SoftDelete]
        public class Archived : StringEntity { }

        public class ArchivedOwner : GuidEntity
        {
            StringCachedReference<Archived> cachedArchived = new StringCachedReference<Archived>();

            public string ArchivedId { get; set; }

            public Archived Archived => cachedArchived.GetOrDefault(ArchivedId);
        }

        public class PlainOwner : GuidEntity
        {
            StringCachedReference<Plain> cachedPlain = new StringCachedReference<Plain>();

            public string PlainId { get; set; }

            public Plain Plain => cachedPlain.GetOrDefault(PlainId);
        }

        public class Holder : GuidEntity
        {
            CachedReference<PlainOwner> cachedOwner = new CachedReference<PlainOwner>();

            public Guid? OwnerId { get; set; }

            public PlainOwner Owner => cachedOwner.GetOrDefault(OwnerId);
        }

        Dictionary<Type, Mock<IDataProvider>> providers;
        Dictionary<Type, List<IEntity>> rows;
        DatabaseConfig config;
        IDatabaseProviderConfig ProviderConfig;
        IDatabase database;

        [SetUp]
        public void SetUp()
        {
            providers = new Dictionary<Type, Mock<IDataProvider>>();
            rows = new Dictionary<Type, List<IEntity>>();
            config = new DatabaseConfig { Cache = new DatabaseConfig.CacheConfig { Enabled = true } };

            var providerConfig = new Mock<IDatabaseProviderConfig>();
            providerConfig.Setup(x => x.Configuration).Returns(() => config);
            providerConfig.Setup(x => x.GetProvider(It.IsAny<Type>())).Returns((Type type) => GetProvider(type).Object);
            ProviderConfig = providerConfig.Object;

            database = new Database(null, ProviderConfig, new Cache(new InMemoryCacheProvider(), ProviderConfig));

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton(database);
            var provider = services.BuildServiceProvider();
            Context.Initialize(provider, () => provider);
        }

        Mock<IDataProvider> GetProvider(Type type)
        {
            if (providers.TryGetValue(type, out var result)) return result;

            result = providers[type] = new Mock<IDataProvider>();
            var records = rows[type] = new List<IEntity>();

            result.Setup(x => x.EntityType).Returns(type);

            result.Setup(x => x.GetList(It.IsAny<IDatabaseQuery>()))
                .Returns((IDatabaseQuery query) => Task.FromResult(records
                    .Where(r => !SoftDeleteAttribute.IsMarked((Entity)r) && Matches(r, query)).ToArray().AsEnumerable()));

            result.Setup(x => x.Get(It.IsAny<object>())).Returns((object id) =>
            {
                // Like the database, a Guid key matches regardless of its text form.
                var record = records.FirstOrDefault(r => r.GetId() is Guid key ?
                    Guid.TryParse(id.ToString(), out var guid) && guid == key : r.GetId().ToString() == id.ToString());
                if (record == null) throw new DataException($"There is no {type.Name} record with the the ID of '{id}'.");
                return Task.FromResult(record);
            });

            return result;
        }

        /// <summary>
        /// Like a case-insensitive database collation, for the simple property criteria used by these tests.
        /// A direct SQL criterion (e.g. for Include) is not evaluated.
        /// </summary>
        static bool Matches(IEntity record, IDatabaseQuery query) =>
            query.Criteria.OfType<Criterion>().Except(c => c is DirectDatabaseCriterion).All(c =>
                string.Equals(record.GetType().GetProperty(c.PropertyName)?.GetValue(record)?.ToString(),
                    c.Value?.ToString(), StringComparison.OrdinalIgnoreCase));

        T Add<T>(T record, bool softDeleted = false) where T : Entity
        {
            GetProvider(typeof(T));
            if (softDeleted) SoftDeleteAttribute.MarkDeleted(record);
            rows[typeof(T)].Add(record);
            return record;
        }

        void VerifyLoads<T>(int lists, int singles)
        {
            GetProvider(typeof(T)).Verify(x => x.GetList(It.IsAny<IDatabaseQuery>()), Times.Exactly(lists));
            GetProvider(typeof(T)).Verify(x => x.Get(It.IsAny<object>()), Times.Exactly(singles));
        }

        [Test]
        public async Task Get_by_id_loads_all_records_in_one_query()
        {
            var active = Add(new Status { ID = "active" });
            var closed = Add(new Status { ID = "closed" });

            Assert.That(await database.Get<Status>("active"), Is.SameAs(active));
            Assert.That(await database.Get<Status>("closed"), Is.SameAs(closed));

            VerifyLoads<Status>(lists: 1, singles: 0);
        }

        [Test]
        public async Task FindById_returns_null_for_a_missing_record()
        {
            Add(new Status { ID = "active" });

            Assert.That(await database.FindById<Status>("missing"), Is.Null);
            Assert.That(await database.FindById<Status>("missing"), Is.Null);
            Assert.That(await database.FindById<Status>("active"), Is.Not.Null);

            // An ID that is not in the loaded list is still looked up by its ID, as before.
            VerifyLoads<Status>(lists: 1, singles: 2);
        }

        [Test]
        public async Task Record_added_after_the_list_was_loaded_is_found_by_id()
        {
            Add(new Status { ID = "active" });
            Assert.That(await database.FindById<Status>("active"), Is.Not.Null);

            var added = Add(new Status { ID = "added" }); // e.g. inserted by another process.

            Assert.That(await database.FindById<Status>("added"), Is.SameAs(added));
            VerifyLoads<Status>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Guid_key_types_load_all_records_and_find_ids_in_any_text_form()
        {
            var region = Add(new Region());
            var other = Add(new Region());

            Assert.That(await database.FindById<Region>(region.ID), Is.SameAs(region));
            Assert.That(await database.FindById<Region>(other.ID.ToString()), Is.SameAs(other));
            VerifyLoads<Region>(lists: 1, singles: 0);

            var uppercase = Add(new Region());
            Assert.That(await database.FindById<Region>(uppercase.ID.ToString().ToUpper()), Is.SameAs(uppercase));
            VerifyLoads<Region>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Record_whose_ToString_reads_the_same_type_does_not_recurse()
        {
            var root = Add(new Node { ID = "root" });
            Add(new Node { ID = "branch", ParentId = "root" });
            var leaf = Add(new Node { ID = "leaf", ParentId = "branch" });

            // Run on another thread: the lookup blocks synchronously if it recurses.
            var lookup = Task.Run(() => database.FindById<Node>("leaf"));
            Assert.That(await Task.WhenAny(lookup, Task.Delay(TimeSpan.FromSeconds(10))), Is.SameAs(lookup));

            Assert.That(await lookup, Is.SameAs(leaf));
            GetProvider(typeof(Node)).Verify(x => x.GetList(It.IsAny<IDatabaseQuery>()), Times.Once);
            Assert.That(leaf.ToString(), Is.EqualTo("root > branch > leaf"));
            Assert.That(await database.FindById<Node>("root"), Is.SameAs(root));
        }

        [Test]
        public async Task Id_in_a_different_case_returns_the_cached_record_without_reloading()
        {
            var gb = Add(new Status { ID = "GB" });
            Assert.That(await database.FindById<Status>("GB"), Is.SameAs(gb));

            // A case-insensitive database returns a new instance of the same record.
            GetProvider(typeof(Status)).Setup(x => x.Get(It.IsAny<object>()))
                .Returns((object id) => Task.FromResult<IEntity>(new Status { ID = "GB" }));

            Assert.That(await database.FindById<Status>("gb"), Is.SameAs(gb));
            Assert.That(await database.FindById<Status>("gb"), Is.SameAs(gb));

            Assert.That(database.Cache.GetList(typeof(Status)), Is.Not.Null);
            VerifyLoads<Status>(lists: 1, singles: 2);
        }

        [Test]
        public void Error_while_loading_all_records_is_not_swallowed()
        {
            GetProvider(typeof(Status)).Setup(x => x.GetList(It.IsAny<IDatabaseQuery>()))
                .ThrowsAsync(new DataException("Timeout"));

            Assert.ThrowsAsync<DataException>(() => database.FindById<Status>("active"));
        }

        [Test]
        public async Task Concurrent_lookups_share_one_load()
        {
            Add(new Status { ID = "one" });
            Add(new Status { ID = "two" });

            GetProvider(typeof(Status)).Setup(x => x.GetList(It.IsAny<IDatabaseQuery>())).Returns(async () =>
            {
                await Task.Delay(100);
                return rows[typeof(Status)].ToArray().AsEnumerable();
            });

            var results = await Task.WhenAll(
                Task.Run(() => database.FindById<Status>("one")),
                Task.Run(() => database.FindById<Status>("two")));

            Assert.That(results.Select(x => x.ID), Is.EqualTo(new[] { "one", "two" }));
            VerifyLoads<Status>(lists: 1, singles: 0);
        }

        [Test]
        public async Task Type_inheriting_from_a_marked_type_loads_records_one_at_a_time()
        {
            var sub = Add(new SubStatus { ID = "sub" });

            Assert.That(await database.FindById<SubStatus>("sub"), Is.SameAs(sub));

            VerifyLoads<SubStatus>(lists: 0, singles: 1);
        }

        [Test]
        public async Task List_with_a_record_changed_while_loading_is_not_cached()
        {
            config.Cache.ConcurrencyAware = true;
            var active = Add(new Status { ID = "active" });

            var changed = false;
            GetProvider(typeof(Status)).Setup(x => x.GetList(It.IsAny<IDatabaseQuery>())).Returns(async () =>
            {
                if (!changed)
                {
                    // Another request saves the record after this query started.
                    changed = true;
                    await Task.Delay(20);
                    database.Cache.UpdateRowVersion(active);
                }

                return rows[typeof(Status)].ToArray().AsEnumerable();
            });

            await database.GetList<Status>();
            await database.GetList<Status>();

            VerifyLoads<Status>(lists: 2, singles: 0);
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task FindById_returns_null_for_an_empty_id_without_querying(string id)
        {
            Assert.That(await database.FindById<Status>(id), Is.Null);

            VerifyLoads<Status>(lists: 0, singles: 0);
        }

        [Test]
        public async Task FindById_matches_the_id_exactly()
        {
            Add(new Status { ID = "active" });

            Assert.That(await database.FindById<Status>("Active"), Is.Null);
        }

        [Test]
        public async Task Get_throws_for_a_missing_record_as_before()
        {
            Add(new Status { ID = "active" });

            Assert.ThrowsAsync<DataException>(() => database.Get<Status>("missing"));
            Assert.That(await database.GetOrDefault<Status>("missing"), Is.Null);
        }

        [Test]
        public void Cached_references_are_served_from_the_loaded_records()
        {
            var active = Add(new Status { ID = "active" });
            var closed = Add(new Status { ID = "closed" });

            var owners = new[]
            {
                new Owner { StatusId = "active" },
                new Owner { StatusId = "closed" },
                new Owner { StatusId = "active" }
            };

            Assert.That(owners.Select(x => x.Status), Is.EqualTo(new[] { active, closed, active }));

            VerifyLoads<Status>(lists: 1, singles: 0);
        }

        [Test]
        public async Task Soft_deleted_record_is_still_found_by_id()
        {
            Add(new Tag { ID = "live" });
            var deleted = Add(new Tag { ID = "retired" }, softDeleted: true);

            Assert.That(await database.FindById<Tag>("retired"), Is.SameAs(deleted));
            Assert.That(await database.FindById<Tag>("retired"), Is.SameAs(deleted));

            // Caching the soft deleted record keeps the loaded list, which it never belonged to.
            Assert.That(database.Cache.GetList(typeof(Tag)), Is.Not.Null);
            Assert.That(await database.FindById<Tag>("live"), Is.Not.Null);

            VerifyLoads<Tag>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Type_without_the_attribute_is_loaded_one_record_at_a_time()
        {
            var plain = Add(new Plain { ID = "one" });

            Assert.That(await database.FindById<Plain>("one"), Is.SameAs(plain));
            Assert.That(await database.FindById<Plain>("missing"), Is.Null);

            VerifyLoads<Plain>(lists: 0, singles: 2);
        }

        [Test]
        public void FindById_does_not_swallow_database_errors()
        {
            GetProvider(typeof(Plain)).Setup(x => x.Get(It.IsAny<object>()))
                .ThrowsAsync(new InvalidOperationException("Connection failed"));

            Assert.ThrowsAsync<InvalidOperationException>(() => database.FindById<Plain>("one"));
        }

        [Test]
        public async Task Records_are_not_loaded_all_together_inside_a_transaction()
        {
            var active = Add(new Status { ID = "active" });

            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                Assert.That(await database.FindById<Status>("active"), Is.SameAs(active));

            VerifyLoads<Status>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Records_are_not_loaded_all_together_when_caching_is_disabled()
        {
            config.Cache.Enabled = false;
            var active = Add(new Status { ID = "active" });

            Assert.That(await database.FindById<Status>("active"), Is.SameAs(active));

            VerifyLoads<Status>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Generated_accessor_parse_and_parse_by_id_share_one_query()
        {
            // The accessor's cached reference outlives a test.
            CachedReferences.InvalidateAll();

            var draft = Add(new Stage { ID = "draft", Name = "Draft" });
            var submitted = Add(new Stage { ID = "submitted", Name = "Submitted" });

            Assert.That(Stage.Draft, Is.SameAs(draft));
            Assert.That(await Stage.Parse("Submitted"), Is.SameAs(submitted));
            Assert.That(await Stage.ParseById("submitted"), Is.SameAs(submitted));
            VerifyLoads<Stage>(lists: 1, singles: 0);

            Assert.That(await Stage.ParseById("missing"), Is.Null);
            VerifyLoads<Stage>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Generated_parse_leaves_text_not_matched_exactly_to_the_database_collation()
        {
            var submitted = Add(new Stage { ID = "submitted", Name = "Submitted" });
            Assert.That(await Stage.Parse("Submitted"), Is.SameAs(submitted));
            VerifyLoads<Stage>(lists: 1, singles: 0);

            // The database decides whether a different case matches, as the Parse() of other types does.
            Assert.That(await Stage.Parse("SUBMITTED"), Is.SameAs(submitted));
            Assert.That(await Stage.Parse("Missing"), Is.Null);
            VerifyLoads<Stage>(lists: 3, singles: 0);
        }

        [Test]
        public async Task Generated_parse_queries_by_text_rather_than_loading_all_records_inside_a_transaction()
        {
            var submitted = Add(new Stage { ID = "submitted", Name = "Submitted" });
            Add(new Stage { ID = "draft", Name = "Draft" });

            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                Assert.That(await database.TryLoadAllRecords<Stage>(), Is.Null);
                Assert.That(await Stage.Parse("Submitted"), Is.SameAs(submitted));
            }

            // One query, filtered by the text.
            VerifyLoads<Stage>(lists: 1, singles: 0);
            Assert.That(database.Cache.GetList(typeof(Stage)), Is.Null);
        }

        [Test]
        public async Task TryLoadAllRecords_loads_the_records_once()
        {
            var one = Add(new Status { ID = "one" });
            var two = Add(new Status { ID = "two" });

            Assert.That(await database.TryLoadAllRecords<Status>(), Is.EquivalentTo(new[] { one, two }));
            Assert.That(await database.TryLoadAllRecords<Status>(), Is.EquivalentTo(new[] { one, two }));
            Assert.That(await database.TryLoadAllRecords<Plain>(), Is.Null);

            VerifyLoads<Status>(lists: 1, singles: 0);
        }

        [Test]
        public void Generated_accessor_by_id_does_not_return_a_soft_deleted_record()
        {
            // The accessor's cached reference outlives a test.
            CachedReferences.InvalidateAll();

            Add(new Phase { ID = "retired" }, softDeleted: true);

            // As when the accessor finds the record by its text, which excludes soft deleted records.
            Assert.That(Phase.Retired, Is.Null);
        }

        [Test]
        public async Task Refreshing_the_cache_invalidates_cached_references()
        {
            var active = Add(new Status { ID = "active" });
            var owner = Add(new Owner { StatusId = "active" });

            Assert.That(owner.Status, Is.SameAs(active));
            Assert.That(owner.Status, Is.SameAs(active));
            VerifyLoads<Status>(lists: 1, singles: 0);

            await database.Refresh();

            // The instance it held is no longer the cached one, so the record is loaded again.
            Assert.That(owner.Status, Is.SameAs(active));
            VerifyLoads<Status>(lists: 2, singles: 0);
        }

        [Test]
        public async Task Records_are_loaded_by_id_within_a_database_context()
        {
            var active = Add(new Status { ID = "active" });

            using (new DatabaseContext("Server=other"))
                Assert.That(await database.FindById<Status>("active"), Is.SameAs(active));

            VerifyLoads<Status>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Records_are_loaded_by_id_when_each_call_gets_a_new_database()
        {
            // As outside a request in the multi-server cache mode.
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddScoped<IDatabase>(_ => new Database(null, ProviderConfig, new Cache(new InMemoryCacheProvider(), ProviderConfig)));
            var provider = services.BuildServiceProvider();
            Context.Initialize(provider, () => null);

            var active = Add(new Status { ID = "active" });

            var first = Context.Current.Database();
            Assert.That(Context.Current.Database(), Is.Not.SameAs(first));

            Assert.That(await first.FindById<Status>("active"), Is.SameAs(active));
            Assert.That(await Context.Current.Database().FindById<Status>("active"), Is.SameAs(active));

            VerifyLoads<Status>(lists: 0, singles: 2);
        }

        /// <summary>
        /// By design, Include loads the associated records from the database, so they are never older than the main
        /// records it loads with them.
        /// </summary>
        [Test]
        public async Task Include_queries_even_when_all_referenced_records_are_cached()
        {
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });
            await database.FindById<Plain>("one"); // Now in the cache.

            GetProvider(typeof(Plain)).Setup(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>())).Returns(new DirectDatabaseCriterion("1 = 1"));

            await database.Of<PlainOwner>().Include(x => x.Plain).GetList();

            Assert.That(owner.Plain, Is.SameAs(one));
            GetProvider(typeof(Plain)).Verify(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>()), Times.Once);
            VerifyLoads<Plain>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Include_queries_as_before_inside_a_transaction()
        {
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });
            await database.FindById<Plain>("one"); // Now in the cache.

            GetProvider(typeof(Plain)).Setup(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>())).Returns(new DirectDatabaseCriterion("1 = 1"));

            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                await database.Of<PlainOwner>().Include(x => x.Plain).GetList();

            GetProvider(typeof(Plain)).Verify(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>()), Times.Once);
            VerifyLoads<Plain>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Include_queries_when_a_cached_referenced_record_is_soft_deleted()
        {
            Add(new Archived { ID = "old" }, softDeleted: true);
            Add(new ArchivedOwner { ArchivedId = "old" });
            Assert.That(await database.FindById<Archived>("old"), Is.Not.Null); // Cached by its ID.

            GetProvider(typeof(Archived)).Setup(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>())).Returns(new DirectDatabaseCriterion("1 = 1"));

            await database.Of<ArchivedOwner>().Include(x => x.Archived).GetList();

            GetProvider(typeof(Archived)).Verify(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>()), Times.Once);
        }

        [Test]
        public async Task Include_queries_the_referenced_records_when_not_all_are_cached()
        {
            var one = Add(new Plain { ID = "one" });
            var two = Add(new Plain { ID = "two" });
            var first = Add(new PlainOwner { PlainId = "one" });
            var second = Add(new PlainOwner { PlainId = "two" });
            await database.FindById<Plain>("one"); // Only one of them is cached.

            GetProvider(typeof(Plain)).Setup(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>())).Returns(new DirectDatabaseCriterion("1 = 1"));

            await database.Of<PlainOwner>().Include(x => x.Plain).GetList();

            Assert.That(first.Plain, Is.SameAs(one));
            Assert.That(second.Plain, Is.SameAs(two));
            VerifyLoads<Plain>(lists: 1, singles: 1);
        }

        [Test]
        public async Task Include_binds_from_the_loaded_records()
        {
            var active = Add(new Status { ID = "active" });
            Add(new Status { ID = "closed" }); // Not referenced by any owner.
            var first = Add(new Owner { StatusId = "active" });
            var second = Add(new Owner { StatusId = "active" });

            var owners = await database.Of<Owner>().Include(x => x.Status).GetList();

            Assert.That(owners, Is.EquivalentTo(new[] { first, second }));
            Assert.That(first.Status, Is.SameAs(active));
            Assert.That(second.Status, Is.SameAs(active));

            VerifyLoads<Status>(lists: 1, singles: 0);
            GetProvider(typeof(Status)).Verify(x => x.GetAssociationInclusionCriteria(It.IsAny<IDatabaseQuery>(),
                It.IsAny<System.Reflection.PropertyInfo>()), Times.Never);
        }

        /// <summary>
        /// Counts the tasks started on it. A read run on it that loads synchronously (RunSync) starts another one.
        /// </summary>
        class CountingScheduler : TaskScheduler
        {
            public int Started;

            protected override void QueueTask(Task task)
            {
                Interlocked.Increment(ref Started);
                ThreadPool.QueueUserWorkItem(_ => TryExecuteTask(task));
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

            protected override IEnumerable<Task> GetScheduledTasks() => null;
        }

        static async Task<(T Result, bool RanSync)> Read<T>(Func<T> read)
        {
            var scheduler = new CountingScheduler();
            var result = await Task.Factory.StartNew(read, CancellationToken.None, TaskCreationOptions.None, scheduler);
            return (result, scheduler.Started > 1);
        }

        [Test]
        public async Task Reading_an_association_that_is_not_included_runs_synchronously()
        {
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });

            var read = await Read(() => owner.Plain);

            Assert.That(read.Result, Is.SameAs(one));
            Assert.That(read.RanSync, Is.True); // What Including() prevents, and this detects.
        }

        [Test]
        public async Task Including_binds_the_association_so_reading_it_does_not_load_it_again()
        {
            config.Cache.Enabled = false; // So any load on reading the association reaches the provider.
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });

            var loaded = await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);
            VerifyLoads<Plain>(lists: 0, singles: 1);

            var read = await Read(() => loaded.Plain);
            Assert.That(read.Result, Is.SameAs(one));
            Assert.That(read.RanSync, Is.False);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Including_binds_nested_associations()
        {
            config.Cache.Enabled = false;
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });
            var holder = Add(new Holder { OwnerId = owner.ID });

            var loaded = await database.Get<Holder>(holder.ID).Including(x => x.Owner.Plain);

            var read = await Read(() => loaded.Owner.Plain);
            Assert.That(read.Result, Is.SameAs(one));
            Assert.That(read.RanSync, Is.False);
            Assert.That(loaded.Owner, Is.SameAs(owner));
            VerifyLoads<PlainOwner>(lists: 0, singles: 1);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Including_binds_a_string_id_that_differs_in_case_from_the_record()
        {
            var gb = Add(new Plain { ID = "GB" });
            var owner = Add(new PlainOwner { PlainId = "gb" });

            // Like a case-insensitive database collation.
            GetProvider(typeof(Plain)).Setup(x => x.Get(It.IsAny<object>())).Returns((object id) =>
                Task.FromResult<IEntity>(rows[typeof(Plain)].Single(r => r.GetId().ToString().Equals(id.ToString(),
                    StringComparison.OrdinalIgnoreCase))));

            var loaded = await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);

            var read = await Read(() => loaded.Plain);
            Assert.That(read.Result, Is.SameAs(gb));
            Assert.That(read.RanSync, Is.False);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Including_is_served_from_the_cache()
        {
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });

            await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);
            var loaded = await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);

            Assert.That((await Read(() => loaded.Plain)).RanSync, Is.False);
            Assert.That(loaded.Plain, Is.SameAs(one));
            VerifyLoads<PlainOwner>(lists: 0, singles: 1);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }

        [Test]
        public async Task Including_binds_inside_a_transaction()
        {
            var one = Add(new Plain { ID = "one" });
            var owner = Add(new PlainOwner { PlainId = "one" });

            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var loaded = await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);

                var read = await Read(() => loaded.Plain);
                Assert.That(read.Result, Is.SameAs(one));
                Assert.That(read.RanSync, Is.False);
            }
        }

        [Test]
        public async Task Reading_an_association_with_no_id_does_not_run_synchronously()
        {
            var owner = Add(new PlainOwner());

            var loaded = await database.Get<PlainOwner>(owner.ID).Including(x => x.Plain);

            var read = await Read(() => loaded.Plain);
            Assert.That(read.Result, Is.Null);
            Assert.That(read.RanSync, Is.False);
            VerifyLoads<Plain>(lists: 0, singles: 0);
        }

        [Test]
        public void Including_a_property_that_is_not_an_association_throws()
        {
            var owner = Add(new PlainOwner { PlainId = "one" });

            Assert.ThrowsAsync<ArgumentException>(() => database.Get<PlainOwner>(owner.ID).Including(x => x.PlainId));
        }

        [Test]
        public async Task Including_returns_null_for_a_missing_record()
        {
            GetProvider(typeof(PlainOwner));

            Assert.That(await database.FindById<PlainOwner>(Guid.NewGuid()).Including(x => x.Plain), Is.Null);
            VerifyLoads<Plain>(lists: 0, singles: 0);
        }

        [Test]
        public async Task Including_binds_the_association_of_a_record_found_by_criteria()
        {
            config.Cache.Enabled = false;
            var one = Add(new Plain { ID = "one" });
            Add(new PlainOwner { PlainId = "one" });

            var loaded = await database.FirstOrDefault<PlainOwner>(x => x.PlainId == "one").Including(x => x.Plain);

            var read = await Read(() => loaded.Plain);
            Assert.That(read.Result, Is.SameAs(one));
            Assert.That(read.RanSync, Is.False);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }

        [Test]
        public async Task IncludeAssociations_binds_the_association_of_a_record_in_hand()
        {
            config.Cache.Enabled = false;
            var one = Add(new Plain { ID = "one" });
            var owner = new PlainOwner { PlainId = "one" };

            await database.IncludeAssociations(owner, x => x.Plain);

            var read = await Read(() => owner.Plain);
            Assert.That(read.Result, Is.SameAs(one));
            Assert.That(read.RanSync, Is.False);
            VerifyLoads<Plain>(lists: 0, singles: 1);
        }
    }
}
