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

            // we need more google results to get this:
            //Assert.IsTrue(results.FuzzyContains("http://notepad-plus.en.softonic.com/download"));

            Assert.IsTrue(results.FuzzyContains("http://filehippo.com/download_notepad/"));
        }

        [TestMethod]
        public void Search7Zip()
        {
            WebSearch webSearch = new WebSearch(new PackageSource("Google", "http://google.com"));
            var results = webSearch.Search("7zip", 30);

            Assert.IsTrue(results.FuzzyContains("http://www.7-zip.org/download.html"));
        }
    }
}
