using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace SmartProvider
{
    public class WebSearch
    {
        private PackageSource _source;

        public WebSearch(PackageSource source)
        {
            _source = source;
        }

        public IEnumerable<string> Search(string name, int howMany)
        {
            if (_source.Location.Contains("google"))
            {
                return GoogleSearch(Uri.EscapeDataString(name + " download")).Where(link => link.Contains("/download")).Where(link => !link.Contains("google")).Take(howMany);
            }
            else
            {
                return GetUrlIHtml(_source.Location + "/search?q=" + Uri.EscapeDataString(name) + "%20download%20location", "/download", _source.Name).Take(howMany);
            }
        }

        private IEnumerable<string> GoogleSearch(string query)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 10);
                var response = httpClient.GetAsync(new Uri("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=8&q=" + query)).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    yield break;
                }

                string content = response.Content.ReadAsStringAsync().Result;

                dynamic results;

                try
                {
                    var json = JsonConvert.DeserializeObject<dynamic>(content);
                    results = json.responseData.results;
                }
                catch (Exception)
                {
                    yield break;
                }

                foreach (var result in results)
                {
                    if (result.unescapedUrl != null)
                    {
                        yield return result.unescapedUrl;
                    }
                }
            }
        }

        private String GetWebContent(string uri)
        {
            byte[] buf = new byte[8192];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream resStream = response.GetResponseStream();
            string tempString = null;
            StringBuilder sb = new StringBuilder();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);

                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
            }
            while (count > 0);

            return sb.ToString();
        }

        private IEnumerable<string> GetUrlIHtml(string uri, string patternPresent, string patternAbsent)
        {
            var content = this.GetWebContent(uri);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            if (htmlDoc.DocumentNode != null)
            {
                var searchEngineLinks = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

                foreach (var searchEngineLink in searchEngineLinks)
                {
                    var value = searchEngineLink.GetAttributeValue("href", null);
                    if (value != null)
                    {
                        if (patternPresent == null || value.Contains(patternPresent))
                        {
                            if (patternAbsent == null || !value.Contains(patternAbsent))
                            {
                                if (value.IsValidUri())
                                {
                                    yield return value;
                                }
                                // Google/Bing appends /url?q=
                                else if (value.Substring(7).IsValidUri())
                                {
                                    yield return value.Substring(7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
