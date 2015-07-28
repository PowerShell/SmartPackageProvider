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
                // FIRST, try direct EXE links
                var exeLinks = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href,'exe')]");

                //TODO: select best exe
                var bestExeLink = (null != exeLinks)?exeLinks[0] : null;

                if (bestExeLink != null)
                {
                    if (null != bestExeLink.Attributes["href"])
                    {
                        return new Uri(uri, bestExeLink.Attributes["href"].Value);
                    }
                    return null;
                }

                // SECOND, look for "Download Now" button, e.g. download.cnet.com
                var downloadNowLinks = htmlDoc.DocumentNode.SelectNodes("//a[span = 'Download Now']");

                var bestDownloadNowLink = (null != downloadNowLinks) ? downloadNowLinks[0] : null;

                if (bestDownloadNowLink != null)
                {
                    var redirectLink = new Uri(uri, (null != bestDownloadNowLink.Attributes["href"]) ? bestDownloadNowLink.Attributes["href"].Value : null);
                }
            }

            return null;
        }
    }
}
