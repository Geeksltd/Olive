using NUnit.Framework;
using System.Collections.Generic;
using Olive;
using System.Linq;

namespace Olive.Tests
{
    [TestFixture]
    public class OliveExtensionTests
    {
        [Test]
        public void Check_AllIndicesOf()
        {
            //string list test
            var stringList = new List<string> { "One", "TWO", "one", "two" };

            var stringResult = stringList.AllIndicesOf("One").ToArray();
            stringResult[0].ShouldEqual(0);

            stringResult = stringList.AllIndicesOf("one").ToArray();
            stringResult[0].ShouldEqual(2);

            stringResult = stringList.AllIndicesOf("TWo").ToArray();
            stringResult.FirstOrDefault().ShouldEqual(-1); //nothing found

            stringResult = stringList.AllIndicesOf<string>("TWo", caseSensitive: false).ToArray();
            stringResult[0].ShouldEqual(1);
            stringResult[1].ShouldEqual(3);

            stringResult = stringList.AllIndicesOf<string>("three", caseSensitive: false).ToArray();
            stringResult.FirstOrDefault().ShouldEqual(-1); //nothing found

            stringResult = stringList.AllIndicesOf<string>("oNe", true).ToArray();
            stringResult[0].ShouldEqual(-1); //nothing found

            stringResult = stringList.AllIndicesOf("oNe").ToArray();
            stringResult[0].ShouldEqual(-1); //nothing found

            //int list test
            var intList = new List<int> { 1, 2, 1, 2, 2 };

            var intResult = intList.AllIndicesOf(1).ToArray();
            intResult[0].ShouldEqual(0);
            intResult[1].ShouldEqual(2);
        }

        [Test]
        public void Check_AllIndicesOf_Criteria()
        {
            //int list test
            var intList = new List<int> { 1, 2, 1, 2, 2 };

            var intResult = intList.AllIndicesOf(x => x > 1).ToArray();
            intResult[0].ShouldEqual(1);
            intResult[1].ShouldEqual(3);
            intResult[2].ShouldEqual(4);

            intResult = intList.AllIndicesOf(x => x > 2).ToArray();
            intResult[0].ShouldEqual(-1); //nothing found
        }

        [Test]
        public void Check_AreItemsUnique()
        {
            var stringList = new List<string> { "AAA", "AAA" };
            stringList.AreItemsUnique().ShouldBeFalse();

            stringList = new List<string> { "AAA", "aaa" };
            stringList.AreItemsUnique().ShouldBeTrue();
            stringList.AreItemsUnique<string>(false).ShouldBeFalse();
            stringList.AreItemsUnique<string>(true).ShouldBeTrue();
            stringList.AreItemsUnique<string>(caseSensitive: true).ShouldBeTrue();
        }
    }
}
