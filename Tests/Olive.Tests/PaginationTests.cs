using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Mvc;

namespace Olive.Tests
{
    [TestFixture]
    public class PaginationTests
    {
        [TestCase("1-50", 1, 50)]
        [TestCase("3-20", 3, 20)]
        [TestCase("3", 3, null)]
        [TestCase("0-50", 1, 50)]
        [TestCase("1-0", 1, null)]
        [TestCase("1-50|1-50", 1, 50)]
        [TestCase("2-50|1-50", 2, 50)]
        [TestCase("1-50,1-50", 1, 50)]
        public void ListPagination_parses_query(string queryInfo, int currentPage, int? pageSize)
        {
            var paging = new ListPagination(null, queryInfo);

            Assert.That(paging.CurrentPage, Is.EqualTo(currentPage));
            Assert.That(paging.PageSize, Is.EqualTo(pageSize));
        }

        [TestCase(1, 50, 0, 50)]
        [TestCase(3, 50, 100, 50)]
        [TestCase(1, null, 0, 100000)]
        [TestCase(2, null, 100000, 100000)]
        [TestCase(0, 10, 0, 10)]
        public void Page_sets_start_index_and_size(int currentPage, int? pageSize, int expectedStart, int expectedSize)
        {
            var query = new Mock<IDatabaseQuery>();
            query.SetupProperty(x => x.PageStartIndex);
            query.SetupProperty(x => x.PageSize);

            var paging = new ListPagination(null, pageSize) { CurrentPage = currentPage };
            query.Object.Page(paging);

            Assert.That(query.Object.PageStartIndex, Is.EqualTo(expectedStart));
            Assert.That(query.Object.PageSize, Is.EqualTo(expectedSize));
        }
    }
}
