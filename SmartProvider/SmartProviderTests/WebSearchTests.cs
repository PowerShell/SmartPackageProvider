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
            WebSearch webSearch = new WebSearch(new PackageSource());
            webSearch.Search("notepad++");
        }
    }
}
