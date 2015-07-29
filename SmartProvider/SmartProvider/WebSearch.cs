using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Linq;

namespace SmartProvider
{
    public class WebSearch
    {
        private PackageSource _source;

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

        private List<Uri> GetUrlIHtml(string uri, string patternPresent, string patternAbsent)
        {
            var content = this.GetWebContent(uri);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            List<Uri> packageDownloadLocation = new List<Uri>();

            if (htmlDoc.DocumentNode != null)
            {
                var searchEngineLinks = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

                foreach (var searchEngineLink in searchEngineLinks)
                {
                    var value = searchEngineLink.GetAttributeValue("href", null);
                    if (value != null)
                    {
                        if (value.Contains(patternPresent) && !value.Contains(patternAbsent))
                        {
                            if (value.IsValidUri())
                            {
                                Uri downloadUri = new Uri(value);
                                packageDownloadLocation.Add(downloadUri);
                            }
                            // Google/Bing appends /url?q=
                            else if (value.Substring(7).IsValidUri())
                            {
                                Uri downloadUri = new Uri(value.Substring(7));
                                packageDownloadLocation.Add(downloadUri);
                            }
                        }
                    }
                }
            }
            
            return packageDownloadLocation.GetRange(0,3);
           
        }

        public WebSearch(PackageSource source)
        {
            _source = source;
        }

        public List<Uri> Search(string name)
        {            
            return this.GetUrlIHtml(_source.Location + "/search?q=" + Uri.EscapeDataString(name) + "%20download%20location", "/download", _source.Name);
        }
    }
}
