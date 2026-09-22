using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Tests
{
    [TestFixture]
    public class InMemoryCacheProviderTests
    {
        public class Item : StringEntity { }

        public class Other : StringEntity { }

        InMemoryCacheProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new InMemoryCacheProvider();

            // A cached reference checks for an open transaction before it keeps a record.
            var database = new Mock<IDatabase>();
            database.Setup(d => d.AnyOpenTransaction()).Returns(false);

            var services = new ServiceCollection();
            services.AddSingleton(database.Object);
            var serviceProvider = services.BuildServiceProvider();
            Context.Initialize(serviceProvider, () => serviceProvider);
        }

        [Test]
        public void Get_returns_null_for_a_type_that_was_never_cached()
        {
            Assert.That(provider.Get(typeof(Other), "missing"), Is.Null);
        }

        [Test]
        public void Add_then_get_returns_the_instance()
        {
            var item = new Item { ID = "a" };
            provider.Add(item);

            Assert.That(provider.Get(typeof(Item), "a"), Is.SameAs(item));
        }

        [Test]
        public void Add_replaces_the_instance_and_invalidates_the_one_it_replaced()
        {
            var first = new Item { ID = "a" };
            var second = new Item { ID = "a" };
            provider.Add(first);

            var reference = new StringCachedReference<Item>();
            var loads = 0;
            Task<Item> load() { loads++; return Task.FromResult((Item)provider.Get(typeof(Item), "a")); }

            Assert.That(reference.Get("a", load), Is.SameAs(first));

            provider.Add(second);

            Assert.That(provider.Get(typeof(Item), "a"), Is.SameAs(second));
            Assert.That(reference.Get("a", load), Is.SameAs(second), "The reference reloads and finds the new instance.");
            Assert.That(loads, Is.EqualTo(2));
        }

        [Test]
        public void Remove_takes_the_instance_out()
        {
            var item = new Item { ID = "a" };
            provider.Add(item);

            provider.Remove(item);

            Assert.That(provider.Get(typeof(Item), "a"), Is.Null);
        }

        [Test]
        public void Remove_of_an_instance_whose_type_was_never_cached_does_nothing()
        {
            Assert.DoesNotThrow(() => provider.Remove(new Other { ID = "a" }));
        }

        [Test]
        public void Remove_type_invalidates_the_cached_references_when_asked()
        {
            var item = new Item { ID = "a" };
            provider.Add(item);

            var reference = new StringCachedReference<Item>();
            var loads = 0;
            Task<Item> load() { loads++; return Task.FromResult(item); }
            reference.Get("a", load);

            provider.Remove(typeof(Item), invalidateCachedReferences: true);
            reference.Get("a", load);

            Assert.That(provider.Get(typeof(Item), "a"), Is.Null);
            Assert.That(loads, Is.EqualTo(2));
        }

        /// <summary>
        /// Pauses a chosen thread inside InvalidateCachedReferences, to put two threads at an exact interleaving.
        /// </summary>
        public class Gated : StringEntity
        {
            internal static Action<Gated> OnInvalidate;

            public override void InvalidateCachedReferences()
            {
                OnInvalidate?.Invoke(this);
                base.InvalidateCachedReferences();
            }
        }

        [TearDown]
        public void TearDown() => Gated.OnInvalidate = null;

        /// <summary>
        /// In the single-server cache mode one provider serves every request, so a save can replace a record while
        /// BulkInsert or BulkUpdate removes its type. The removal walks the type's records to invalidate them; the walk
        /// must survive the replacement landing half way through it.
        /// </summary>
        [Test]
        public async Task Removing_a_type_survives_a_record_replaced_while_it_is_being_invalidated()
        {
            provider.Add(new Gated { ID = "1" });
            provider.Add(new Gated { ID = "2" });

            var writerPaused = new ManualResetEventSlim();
            var writerDone = new ManualResetEventSlim();
            var removerInWalk = 0;
            int writerThread = 0;

            Gated.OnInvalidate = _ =>
            {
                if (Environment.CurrentManagedThreadId == writerThread)
                {
                    // The writer replacing record 1 waits here until the remover is walking the records.
                    writerPaused.Set();
                    Assert.That(SpinWait.SpinUntil(() => Volatile.Read(ref removerInWalk) == 1, 5000), "The remover never started its walk.");
                }
                else if (Interlocked.CompareExchange(ref removerInWalk, 1, 0) == 0)
                {
                    // The remover, on its first record, waits until the writer has finished replacing record 1.
                    Assert.That(writerDone.Wait(5000), "The writer never finished.");
                }
            };

            var writer = Task.Run(() =>
            {
                writerThread = Environment.CurrentManagedThreadId;
                provider.Add(new Gated { ID = "1" });
                writerDone.Set();
            });

            Assert.That(writerPaused.Wait(5000), "The writer never reached the record it replaces.");

            Assert.DoesNotThrow(() => provider.Remove(typeof(Gated), invalidateCachedReferences: true));
            await writer;
        }
    }
}
