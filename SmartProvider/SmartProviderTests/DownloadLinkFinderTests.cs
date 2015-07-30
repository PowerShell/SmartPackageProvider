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
            string expected = @"http://software-files-a.cnet.com/s/software/14/43/26/20/npp.6.8.Installer.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.IsTrue(uri.ToString().Contains(expected));
        }

        [TestMethod]
        public void NotepadPlusPlus3()
        {
            string searchResult = @"http://notepad-plus.en.softonic.com/download";
            string expected = @"npp-6-8-Installer.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.IsTrue(uri.ToString().Contains(expected));
        }

        [TestMethod]
        public void NotepadPlusPlus4()
        {
            string searchResult = @"http://download.cnet.com/Notepad/3000-2352_4-10327521.html&amp;sa=U&amp;ved=0CDsQFjAHahUKEwjHhOOTyP_GAhWTMogKHYyfDLc&amp;usg=AFQjCNHsZQSNDrak4OrD5fTwyPAzuFTUGA";
            string expected = @"npp.6.8.Installer.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.IsTrue(uri.ToString().Contains(expected));
        }

        [TestMethod]
        public void Crawl7Zip()
        {
            string searchResult = @"http://www.7-zip.org/download.html";
            string expected = @"7z1505.exe";

            var uri = DownloadLinkFinder.GetDownloadLink(new Uri(searchResult)).Result;
            Assert.IsTrue(uri.ToString().Contains(expected));
        }
    }
}
