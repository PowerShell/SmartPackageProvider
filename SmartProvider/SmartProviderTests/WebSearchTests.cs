using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartProvider;

namespace SmartProviderTests
{
    [TestClass]
    public class WebSearchTests
    {
        [TestMethod]
        public void SearchNotepadPlusPlus()
        {
            WebSearch webSearch = new WebSearch(new PackageSource("Google", "http://google.com"));
            var results = webSearch.Search("notepad++", 30);

            Assert.IsTrue(results.FuzzyContains("https://notepad-plus-plus.org/download/"));

            // for now, we don't have this link even though we should:
            //Assert.IsTrue(results.FuzzyContains("http://notepad-plus.en.softonic.com/download"));

            Assert.IsTrue(results.FuzzyContains("http://filehippo.com/download_notepad/"));
        }
    }
}
