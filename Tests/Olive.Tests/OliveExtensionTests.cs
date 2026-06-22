using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using Olive.Mvc;
using System.Collections.Generic;
using Olive;
using System.Linq;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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

        [Test]
        public void Check_Distinct()
        {
            var stringList = new List<string> { "AAA", "bbb", "aaa", "AAA" };

            stringList.Distinct(x => x).Count().ShouldEqual(3);

            stringList.Distinct(x => x.ToUpper()).Count().ShouldEqual(2);

            var intList = new List<int>() { 30, 50, 60, 60, 70 };

            intList.Distinct(x => x).Count().ShouldEqual(4);

            //TODO: Should we consider condition?
            //intList.Distinct(x => x > 70).Count().ShouldEqual(0);
        }

        [Test]
        public void Check_Except()
        {
            var stringList = new List<string> { "AAA", "bbb", "aaa", "AAA" };

            stringList.Except(x => x == "AAA").Count().ShouldEqual(2);

            //stringList.Except(caseSensitive: false, items: new string[] { "AAA" }).Count().ShouldEqual(1);
            //stringList.Except(false, new string[] { "AAA" }).Count().ShouldEqual(1);

            //stringList.Except(true, new string[] { "AAA" }).Count().ShouldEqual(2);
        }

        [Test]
        public void Check_GetElementAfter()
        {
            var stringList = new List<string> { "Geeks", "Apple", "Computer" };

            stringList.GetElementAfter("Geeks").ShouldEqual("Apple");
            stringList.GetElementAfter("geeks", false).ShouldEqual("Apple");
        }

        [Test]
        public void Check_GetElementBefore()
        {
            var stringList = new List<string> { "Geeks", "Apple", "Computer" };

            stringList.GetElementBefore("Apple").ShouldEqual("Geeks");
            stringList.GetElementBefore("apple", false).ShouldEqual("Geeks");
        }

        [Test]
        public void Check_IndexOf()
        {
            var stringList = new List<string> { "Geeks", "Apple", "Computer" };

            stringList.IndexOf("Apple").ShouldEqual(1);
            stringList.IndexOf("Computer").ShouldEqual(2);

            stringList.IndexOf("computer").ShouldEqual(-1);
        }

        [Test]
        public void Check_Intersects()
        {
            var stringList1 = new List<string> { "Geeks", "Apple", "Computer" };
            var stringList2 = new List<string> { "One", "Apple", "Two" };
            var stringList3 = new List<string> { "One", "apple", "Two" };

            stringList1.Intersects(stringList2).ShouldBeTrue();
            stringList1.Intersects(stringList3).ShouldBeFalse();
            //TODO: Uncomment after merging related PRs
            //stringList1.Intersects(stringList3, caseSensitive: false).ShouldBeTrue();
            //stringList1.Intersects(stringList3, false).ShouldBeTrue();
            //stringList1.Intersects(stringList3, true).ShouldBeFalse();
        }

        [Test]
        public void Check_IsEquivalentTo()
        {
            var stringList1 = new List<string> { "Geeks", "Apple", "Computer" };
            var stringList2 = new List<string> { "Geeks", "apple", "Computer" };

            stringList1.IsEquivalentTo(stringList2).ShouldBeFalse();
            //TODO: Uncomment after merging related PRs
            //stringList1.IsEquivalentTo(stringList2, true).ShouldBeFalse();
            //stringList1.IsEquivalentTo(stringList2, false).ShouldBeTrue();
        }

        [Test]
        public void Check_LacksAll()
        {
            var stringList1 = new List<string> { "Geeks", "Apple", "Computer" };
            var stringList2 = new List<string> { "geeks", "apple" };

            stringList1.LacksAll(stringList2).ShouldBeTrue();
            stringList1.LacksAll(stringList2, true).ShouldBeTrue();

            stringList1.LacksAll(stringList2, false).ShouldBeFalse();
        }

        static string GetHtml(HtmlString html)
        {
            using var writer = new StringWriter();
            html.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        [Test]
        public void Raw_DefaultSanitize_RemovesScriptTags()
        {
            var input = "<p>Hello</p><script>alert('xss')</script>";

            var result = GetHtml(input.Raw());

            Assert.That(result, Does.Not.Contain("<script"));
            Assert.That(result, Does.Contain("<p>Hello</p>"));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesEventHandlers()
        {
            var input = "<p id=\"p1\" class=\"text\" onclick=\"evil()\">Click</p>";

            var result = GetHtml(input.Raw(sanitize: true));

            result.ShouldEqual("<p id=\"p1\" class=\"text\">Click</p>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesJavascriptUrls()
        {
            var input = "<a href=\"javascript:alert(1)\">link</a>";

            var result = GetHtml(input.Raw(sanitize: true));

            result.ShouldEqual("<a>link</a>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_KeepsAllowedLinkSchemes()
        {
            var mailto = GetHtml("<a href=\"mailto:test@example.com\">email</a>".Raw());
            var tel = GetHtml("<a href=\"tel:+1234567890\">phone</a>".Raw());
            var http = GetHtml("<a href=\"http://example.com\">http</a>".Raw());
            var https = GetHtml("<a href=\"https://example.com\">https</a>".Raw());

            mailto.ShouldEqual("<a href=\"mailto:test@example.com\">email</a>");
            tel.ShouldEqual("<a href=\"tel:+1234567890\">phone</a>");
            http.ShouldEqual("<a href=\"http://example.com\">http</a>");
            https.ShouldEqual("<a href=\"https://example.com\">https</a>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_KeepsStyleAttribute()
        {
            var input = "<span style=\"color:red\">styled</span>";

            var result = GetHtml(input.Raw(sanitize: true));

            Assert.That(result, Does.Contain("style="));
            Assert.That(result, Does.Contain("styled"));
            Assert.That(result, Does.Not.Contain("javascript:"));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesScriptElementButKeepsChildText()
        {
            var result = GetHtml("<p>Hi</p><script>removed()</script>".Raw());

            result.ShouldEqual("<p>Hi</p>removed()");
        }

        [Test]
        public void Raw_WithSanitizeFalse_PreservesUnsafeHtml()
        {
            var input = "<p onclick=\"evil()\">unsafe</p><script>alert(1)</script>";

            var result = GetHtml(input.Raw(sanitize: false));

            result.ShouldEqual(input);
        }

        [Test]
        public void Raw_NullOrEmpty_ReturnsEmptyHtmlString()
        {
            GetHtml(((string)null).Raw()).ShouldEqual(string.Empty);
            GetHtml(string.Empty.Raw()).ShouldEqual(string.Empty);
        }

        [Test]
        public async Task Raw_Task_DefaultSanitize_RemovesScriptTags()
        {
            var result = GetHtml(await Task.FromResult("<b>ok</b><script>x</script>").Raw());

            Assert.That(result, Does.Not.Contain("<script"));
            Assert.That(result, Does.Contain("<b>ok</b>"));
        }

        [Test]
        public async Task Raw_Task_WithSanitizeFalse_PreservesUnsafeHtml()
        {
            var input = "<img onerror=\"evil()\" src=\"x\">";

            var result = GetHtml(await Task.FromResult(input).Raw(sanitize: false));

            result.ShouldEqual(input);
        }
    }
}
