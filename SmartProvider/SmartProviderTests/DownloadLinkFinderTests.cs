using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartProvider;

namespace SmartProviderTests
{
    [TestClass]
    public class DownloadLinkFinderTests
    {
        [TestMethod]
        public void NotepadPlusPlus()
        {
            string searchResult = @"https://notepad-plus-plus.org/download/";
            string expected = @"https://notepad-plus-plus.org/repository/6.x/6.8/npp.6.8.Installer.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.AreEqual(new Uri(expected), uri);
        }

        [TestMethod]
        public void NotepadPlusPlus2()
        {
            string searchResult = @"http://download.cnet.com/Notepad/3000-2352_4-10327521.html";
            string expected = @"http://download.cnet.com/Notepad/3001-2352_4-10327521.html?hasJs=n&hlndr=1";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.AreEqual(new Uri(expected), uri);
        }

        [TestMethod]
        public void NotepadPlusPlus3()
        {
            string searchResult = @"http://notepad-plus.en.softonic.com/download";
            string expected = @"npp-6-8-Installer.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.IsTrue(uri.AbsolutePath.Contains(expected));
        }
    }
}
