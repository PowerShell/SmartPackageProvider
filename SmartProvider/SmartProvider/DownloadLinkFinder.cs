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
                var bestExeLink = (null != exeLinks) ? exeLinks[0] : null;

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
                        response = await httpClient.GetAsync(redirectLink);

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
                                var cnetMetaRefreshLink = metaRefresh[0].GetAttributeValue("content", null).Substring(6);
                                var cnetMetaRefreshUri = new Uri(cnetMetaRefreshLink);
                                if (await CheckPotentialExe(cnetMetaRefreshUri))
                                {
                                    return cnetMetaRefreshUri;
                                }
                            }

                            // search for cnet-specific tag
                            var hiddenDiv = htmlDoc.DocumentNode.SelectNodes("//div[@data-download-now-url]");
                            if (hiddenDiv != null && hiddenDiv.Count > 0)
                            {
                                var link = hiddenDiv[0].Attributes["data-download-now-url"];
                                if (link != null)
                                {
                                    if (link.Value.IsValidUri())
                                    {
                                        var linkAddress = new Uri(link.Value);
                                        if (await CheckPotentialExe(linkAddress))
                                        {
                                            return linkAddress;
                                        }
                                    }
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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = await httpClient.SendAsync(request);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            IEnumerable<string> values;
            if (response.Content.Headers.TryGetValues("Content-Type", out values))
            {
                var contentType = values.First();
                if (contentType.Contains("application"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
