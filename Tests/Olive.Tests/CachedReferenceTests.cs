using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Tests
{
    [TestFixture]
    public class CachedReferenceTests
    {
        public class Country : StringEntity { }

        public class Region : GuidEntity { }

        public class Level : IntEntity { }

        public class Applicant : GuidEntity
        {
            StringCachedReference<Country> cachedCountry = new StringCachedReference<Country>();
            CachedReference<Region> cachedRegion = new CachedReference<Region>();

            public string CountryId { get; set; }

            public Country Country => cachedCountry.GetOrDefault(CountryId);

            public Task<Country> GetCountryAsync() => cachedCountry.GetOrDefaultAsync(CountryId);

            public Guid? RegionId { get; set; }

            public Region Region => cachedRegion.GetOrDefault(RegionId);
        }

        Mock<IDatabase> database;
        bool inTransaction;
        Dictionary<object, IEntity> records;

        [SetUp]
        public void SetUp()
        {
            inTransaction = false;
            LoaderCalls = 0;
            records = new Dictionary<object, IEntity>();

            database = new Mock<IDatabase>();
            database.Setup(d => d.AnyOpenTransaction()).Returns(() => inTransaction);
            database.Setup(d => d.GetOrDefault<Country>(It.IsAny<object>()))
                .Returns((object id) => Task.FromResult((Country)records.GetOrDefault(id)));
            database.Setup(d => d.Get<Country>(It.IsAny<string>()))
                .Returns((string id) => Task.FromResult((Country)records[id]));
            database.Setup(d => d.GetOrDefault<Region>(It.IsAny<object>()))
                .Returns((object id) => Task.FromResult((Region)records.GetOrDefault(id)));

            var services = new ServiceCollection();
            services.AddSingleton(database.Object);
            var provider = services.BuildServiceProvider();
            Context.Initialize(provider, () => provider);
        }

        Country AddCountry(string id)
        {
            var result = new Country { ID = id };
            records[id] = result;
            return result;
        }

        Region AddRegion()
        {
            var result = new Region();
            records[result.ID.ToString()] = result;
            return result;
        }

        void VerifyCountryLoads(string id, int times) =>
            database.Verify(d => d.GetOrDefault<Country>(id), Times.Exactly(times));

        #region StringCachedReference

        [Test]
        public void String_reference_loads_once_per_id()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            Assert.That(applicant.Country, Is.SameAs(gb));
            Assert.That(applicant.Country, Is.SameAs(gb));

            VerifyCountryLoads("GB", 1);
        }

        [Test]
        public async Task String_reference_async_loads_once_per_id()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            Assert.That(await applicant.GetCountryAsync(), Is.SameAs(gb));
            Assert.That(await applicant.GetCountryAsync(), Is.SameAs(gb));

            VerifyCountryLoads("GB", 1);
        }

        [Test]
        public void String_reference_reloads_when_id_changes()
        {
            var gb = AddCountry("GB");
            var fr = AddCountry("FR");
            var applicant = new Applicant { CountryId = "GB" };

            Assert.That(applicant.Country, Is.SameAs(gb));

            applicant.CountryId = "FR";
            Assert.That(applicant.Country, Is.SameAs(fr));

            VerifyCountryLoads("GB", 1);
            VerifyCountryLoads("FR", 1);
        }

        [Test]
        public void String_reference_treats_ids_differing_in_case_as_different()
        {
            var upper = AddCountry("AB");
            var lower = AddCountry("ab");
            var applicant = new Applicant { CountryId = "AB" };

            Assert.That(applicant.Country, Is.SameAs(upper));

            applicant.CountryId = "ab";
            Assert.That(applicant.Country, Is.SameAs(lower));

            VerifyCountryLoads("ab", 1);
        }

        [TestCase(null)]
        [TestCase("")]
        public void String_reference_returns_null_for_empty_id_without_loading(string id)
        {
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get(id), Is.Null);
            Assert.That(reference.GetOrDefault(id), Is.Null);

            database.Verify(d => d.Get<Country>(It.IsAny<string>()), Times.Never);
            database.Verify(d => d.GetOrDefault<Country>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void String_reference_get_uses_get_and_caches()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB"), Is.SameAs(gb));
            Assert.That(reference.Get("GB"), Is.SameAs(gb));

            database.Verify(d => d.Get<Country>("GB"), Times.Once);
        }

        [Test]
        public void String_reference_reloads_after_target_is_invalidated()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            Assert.That(applicant.Country, Is.SameAs(gb));

            gb.InvalidateCachedReferences();
            Assert.That(applicant.Country, Is.SameAs(gb));

            VerifyCountryLoads("GB", 2);
        }

        [Test]
        public void String_reference_does_not_keep_value_loaded_in_transaction()
        {
            AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };
            inTransaction = true;

            applicant.Country.ShouldNotBeNull();
            applicant.Country.ShouldNotBeNull();

            VerifyCountryLoads("GB", 2);
        }

        [Test]
        public void String_reference_reloads_after_all_references_are_invalidated()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            Assert.That(applicant.Country, Is.SameAs(gb));

            CachedReferences.InvalidateAll();
            Assert.That(applicant.Country, Is.SameAs(gb));

            VerifyCountryLoads("GB", 2);
        }

        #endregion

        #region Loading through a custom loader

        int LoaderCalls;

        Task<Country> Load(string id)
        {
            LoaderCalls++;
            return Task.FromResult((Country)records.GetOrDefault(id));
        }

        void VerifyNoDatabaseLoads()
        {
            database.Verify(d => d.Get<Country>(It.IsAny<string>()), Times.Never);
            database.Verify(d => d.GetOrDefault<Country>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Loader_is_called_only_when_the_record_is_not_cached()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));
            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            Assert.That(LoaderCalls, Is.EqualTo(1));
            VerifyNoDatabaseLoads();
        }

        [Test]
        public async Task Loader_is_called_only_when_the_record_is_not_cached_async()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(await reference.GetAsync("GB", () => Load("GB")), Is.SameAs(gb));
            Assert.That(await reference.GetAsync("GB", () => Load("GB")), Is.SameAs(gb));

            Assert.That(LoaderCalls, Is.EqualTo(1));
            VerifyNoDatabaseLoads();
        }

        [Test]
        public void Loader_is_called_again_after_its_record_is_invalidated()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            gb.InvalidateCachedReferences();
            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void Loader_is_called_again_after_all_references_are_invalidated()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            CachedReferences.InvalidateAll();
            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void Loader_result_of_null_is_not_cached()
        {
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.Null);
            Assert.That(reference.Get("GB", () => Load("GB")), Is.Null);

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void Loader_result_is_not_kept_in_a_transaction()
        {
            AddCountry("GB");
            var reference = new StringCachedReference<Country>();
            inTransaction = true;

            reference.Get("GB", () => Load("GB")).ShouldNotBeNull();
            reference.Get("GB", () => Load("GB")).ShouldNotBeNull();

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Loader_is_not_called_for_an_empty_id(string id)
        {
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get(id, () => Load(id)), Is.Null);

            Assert.That(LoaderCalls, Is.EqualTo(0));
            VerifyNoDatabaseLoads();
        }

        [Test]
        public void Loader_reloads_when_the_id_changes()
        {
            var gb = AddCountry("GB");
            var fr = AddCountry("FR");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));
            Assert.That(reference.Get("FR", () => Load("FR")), Is.SameAs(fr));

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void Guid_reference_loads_through_a_loader()
        {
            var region = AddRegion();
            var reference = new CachedReference<Region>();
            var calls = 0;

            Task<Region> load() { calls++; return Task.FromResult(region); }

            Assert.That(reference.Get(region.ID, load), Is.SameAs(region));
            Assert.That(reference.Get(region.ID, load), Is.SameAs(region));

            Assert.That(calls, Is.EqualTo(1));
            database.Verify(d => d.GetOrDefault<Region>(It.IsAny<object>()), Times.Never);
        }

        #endregion

        #region Invalidation

        /// <summary>
        /// Loads a record through a reference that is then dropped, and returns a handle that shows whether the
        /// reference has been collected. Kept out of line, so that no local of the caller keeps it alive.
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static WeakReference LoadThroughDroppedReference(Country country)
        {
            var reference = new StringCachedReference<Country>();
            reference.Get(country.ID, () => Task.FromResult(country));
            return new WeakReference(reference);
        }

        [Test]
        public void A_record_keeps_no_reference_to_the_references_that_loaded_it()
        {
            var gb = AddCountry("GB");

            // Each of these would stay reachable if the record kept a list of the references to it.
            var dropped = Enumerable.Range(0, 20).Select(_ => LoadThroughDroppedReference(gb)).ToArray();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(dropped.Count(x => x.IsAlive), Is.Zero);
            GC.KeepAlive(gb);
        }

        [Test]
        public void Every_reference_to_a_record_reloads_after_it_is_invalidated()
        {
            var gb = AddCountry("GB");
            var references = Enumerable.Range(0, 3).Select(_ => new StringCachedReference<Country>()).ToArray();

            foreach (var reference in references) reference.Get("GB", () => Load("GB"));
            gb.InvalidateCachedReferences();
            foreach (var reference in references) reference.Get("GB", () => Load("GB"));

            Assert.That(LoaderCalls, Is.EqualTo(6));
        }

        [Test]
        public void A_reference_that_reloads_the_same_instance_after_it_is_invalidated_is_current_again()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();

            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            gb.InvalidateCachedReferences();

            // The reload returns the same instance, which must count as current from then on.
            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));
            Assert.That(reference.Get("GB", () => Load("GB")), Is.SameAs(gb));

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void Invalidating_a_clone_invalidates_the_references_to_its_original()
        {
            var gb = AddCountry("GB");
            var reference = new StringCachedReference<Country>();
            reference.Get("GB", () => Load("GB"));

            ((Country)gb.Clone()).InvalidateCachedReferences();
            reference.Get("GB", () => Load("GB"));

            Assert.That(LoaderCalls, Is.EqualTo(2));
        }

        [Test]
        public void A_reference_bound_by_include_is_invalidated_with_its_record()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            Bind("Country", new[] { gb }, new[] { applicant });
            Assert.That(applicant.Country, Is.SameAs(gb));
            VerifyCountryLoads("GB", 0);

            gb.InvalidateCachedReferences();

            Assert.That(applicant.Country, Is.SameAs(gb));
            VerifyCountryLoads("GB", 1);
        }

        [Test]
        public void A_reference_bound_by_include_inside_a_transaction_is_not_invalidated_with_its_record()
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = "GB" };

            inTransaction = true;
            Bind("Country", new[] { gb }, new[] { applicant });
            inTransaction = false;

            gb.InvalidateCachedReferences();

            // By design: it stays until its ID changes or all cached references are invalidated.
            Assert.That(applicant.Country, Is.SameAs(gb));
            VerifyCountryLoads("GB", 0);
        }

        #endregion

        #region Guid CachedReference (unchanged behaviour)

        [Test]
        public void Guid_reference_loads_once_per_id()
        {
            var region = AddRegion();
            var applicant = new Applicant { RegionId = region.ID };

            Assert.That(applicant.Region, Is.SameAs(region));
            Assert.That(applicant.Region, Is.SameAs(region));

            database.Verify(d => d.GetOrDefault<Region>(region.ID.ToString()), Times.Once);
        }

        [Test]
        public void Guid_reference_returns_null_for_null_id_without_loading()
        {
            Assert.That(new Applicant().Region, Is.Null);

            database.Verify(d => d.GetOrDefault<Region>(It.IsAny<object>()), Times.Never);
        }

        [TestCase(typeof(CachedReference<Region>))]
        [TestCase(typeof(CachedReference<int, Level>))]
        [TestCase(typeof(StringCachedReference<Country>))]
        public void Bind_is_discoverable_for_include(Type referenceType)
        {
            var bind = referenceType.GetMethod("Bind", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(bind, Is.Not.Null);
        }

        [Test]
        public void Guid_reference_public_signatures_are_unchanged()
        {
            var type = typeof(CachedReference<int, Level>);

            foreach (var name in new[] { "Get", "GetOrDefault", "GetAsync", "GetOrDefaultAsync" })
            {
                var method = type.GetMethod(name, new[] { typeof(int?) });
                Assert.That(method, Is.Not.Null, name);
                Assert.That(method.DeclaringType.GetGenericTypeDefinition(), Is.EqualTo(typeof(CachedReference<,>)), name);
            }
        }

        #endregion

        #region Include binding

        static void Bind(string association, IEnumerable<IEntity> associated, IEnumerable<IEntity> main, bool ignoreUnreferenced = false)
        {
            var property = typeof(Applicant).GetProperty(association);
            var cachedField = typeof(Applicant).GetField("cached" + association, BindingFlags.NonPublic | BindingFlags.Instance);

            // BindAssociations is internal, and the test assembly has no access to internals.
            var bind = typeof(AssociationInclusion).GetMethod("BindAssociations", BindingFlags.NonPublic | BindingFlags.Instance);

            try { bind.Invoke(AssociationInclusion.Create(property), new object[] { cachedField, associated, main, ignoreUnreferenced }); }
            catch (TargetInvocationException ex) { throw ex.InnerException; }
        }

        [Test]
        public void Include_binds_string_associations_by_exact_id()
        {
            var gb = AddCountry("GB");
            var fr = AddCountry("FR");
            var first = new Applicant { CountryId = "GB" };
            var second = new Applicant { CountryId = "FR" };
            var third = new Applicant { CountryId = "GB" };

            Bind("Country", new[] { gb, fr }, new[] { first, second, third });

            Assert.That(first.Country, Is.SameAs(gb));
            Assert.That(second.Country, Is.SameAs(fr));
            Assert.That(third.Country, Is.SameAs(gb));

            database.Verify(d => d.GetOrDefault<Country>(It.IsAny<object>()), Times.Never);
        }

        [TestCase("gb")]
        [TestCase("GB ")]
        public void Include_leaves_non_exact_string_match_unbound_without_throwing(string foreignKey)
        {
            var gb = AddCountry("GB");
            var applicant = new Applicant { CountryId = foreignKey };

            Assert.DoesNotThrow(() => Bind("Country", new[] { gb }, new[] { applicant }));

            applicant.Country.ShouldBeNull(); // Loaded on demand; the mock database is case and space sensitive.
            VerifyCountryLoads(foreignKey, 1);
        }

        [Test]
        public void Include_does_not_merge_string_ids_differing_in_case()
        {
            var upper = AddCountry("AB");
            var lower = AddCountry("ab");
            var first = new Applicant { CountryId = "AB" };
            var second = new Applicant { CountryId = "ab" };

            Bind("Country", new[] { upper, lower }, new[] { first, second });

            Assert.That(first.Country, Is.SameAs(upper));
            Assert.That(second.Country, Is.SameAs(lower));
        }

        [Test]
        public void Include_binds_guid_associations()
        {
            var region = AddRegion();
            var applicant = new Applicant { RegionId = region.ID };

            Bind("Region", new[] { region }, new[] { applicant });

            Assert.That(applicant.Region, Is.SameAs(region));
            database.Verify(d => d.GetOrDefault<Region>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Include_still_throws_for_unmatched_guid_association()
        {
            var region = AddRegion();
            var applicant = new Applicant { RegionId = Guid.NewGuid() };

            var error = Assert.Throws<Exception>(() => Bind("Region", new[] { region }, new[] { applicant }));
            Assert.That(error.Message, Does.StartWith("Database include binding failed."));
        }

        [Test]
        public void Include_skips_unmatched_guid_association_when_paged()
        {
            var region = AddRegion();
            var applicant = new Applicant { RegionId = Guid.NewGuid() };

            Assert.DoesNotThrow(() => Bind("Region", new[] { region }, new[] { applicant }, ignoreUnreferenced: true));
        }

        #endregion
    }
}
