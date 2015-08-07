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
        public async static Task<Uri> GetDownloadLink(string link)
        {
            if (!link.IsValidUri())
            {
                return null;
            }

            var uri = new Uri(link);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome / 44.0.2403.125 Safari / 537.36");
                httpClient.Timeout = new TimeSpan(0, 0, 20);
                var response = await httpClient.GetAsync(uri);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

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

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                return null;
                            }

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
                                    var downloadNowLink = hiddenDiv[0].Attributes["data-download-now-url"];
                                    if (downloadNowLink != null)
                                    {
                                        if (downloadNowLink.Value.IsValidUri())
                                        {
                                            var downloadNowAddress = new Uri(downloadNowLink.Value);
                                            if (await CheckPotentialExe(downloadNowAddress))
                                            {
                                                return downloadNowAddress;
                                            }
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
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 10);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, uri);
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }

                IEnumerable<string> values;
                if (response.Content.Headers.TryGetValues("Content-Type", out values))
                {
                    var contentType = values.First();
                    if (contentType.Contains("application"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
