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
                        var cleanLink = HtmlEntity.DeEntitize(bestExeLink.Attributes["href"].Value);
                        var potentialUrl = new Uri(uri, cleanLink);
                        if (await CheckPotentialExe(potentialUrl))
                        {
                            return potentialUrl;
                        }
                    }
                    return null;
                }

                // SECOND, look for "Download Now" button, e.g. download.cnet.com
                var downloadNowLinks = htmlDoc.DocumentNode.SelectNodes("//a[span = 'Download Now']");

                var bestDownloadNowLink = (null != downloadNowLinks) ? downloadNowLinks[0] : null;

                if (bestDownloadNowLink != null)
                {
                    var redirectLink = new Uri(uri, (null != bestDownloadNowLink.Attributes["href"]) ? bestDownloadNowLink.Attributes["href"].Value : null);
                    if (await CheckPotentialExe(redirectLink))
                    {
                        return redirectLink;
                    }
                    else
                    {
                        // THIRD, check for meta refresh
                        response = await httpClient.GetAsync(uri);

                        //will throw an exception if not successful
                        response.EnsureSuccessStatusCode();

                        content = await response.Content.ReadAsStringAsync();

                        htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(content);

                        if (htmlDoc.DocumentNode != null)
                        {
                            // search for meta refresh
                            var metaRefresh = htmlDoc.DocumentNode.SelectNodes("//meta[contains(@http-equiv,'refresh')]");
                            if (metaRefresh != null && metaRefresh.Count > 0 && metaRefresh[0] != null)
                            {
                                var metaRefreshLink = metaRefresh[0].GetAttributeValue("url", null);
                                var metaRefreshUri = new Uri(metaRefreshLink);
                                if (await CheckPotentialExe(metaRefreshUri))
                                {
                                    return metaRefreshUri;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private async static Task<bool> CheckPotentialExe(Uri uri)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(uri);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            //string content = await response.Content.ReadAsStringAsync();

            IEnumerable<string> values;
            if (response.Content.Headers.TryGetValues("Content-Type", out values))
            {
                var contentType = values.First();
                if (contentType == "application/x-msdos-program" || contentType == "application/x-msdownload")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
