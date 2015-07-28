using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartProvider
{
    public static class DownloadLinkFinder
    {
        public async static Task<Uri> GetDownloadLink(Uri uri)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(uri);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            if (htmlDoc.DocumentNode != null)
            {
                var exeLinks = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href,'exe')]");

                //TODO: select best exe
                var bestExeLink = exeLinks?[0];

                if (bestExeLink != null)
                {
                    return new Uri(uri, bestExeLink.Attributes["href"]?.Value);
                }
            }

            return null;
            //return new Uri("https://notepad-plus-plus.org/repository/6.x/6.8/npp.6.8.Installer.exe");
        }
    }
}
