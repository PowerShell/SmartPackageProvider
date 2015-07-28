using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace SmartProvider
{
    class WebSearch
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
            MatchCollection matches = Regex.Matches(this.GetWebContent(uri),
                @"\b((https?|ftp|file)://|(www|ftp)\.)[-A-Z0-9+&@#/%?=~_|$!:,.;]*[A-Z0-9+&@#/%=~_|$]", RegexOptions.IgnoreCase);

            List<Uri> packageDownloadLocation = new List<Uri>();

            foreach (Match match in matches)
            {
                if (match.ToString().Contains(patternPresent) && !match.ToString().Contains(patternAbsent))
                {
                    Uri downloadUri = new Uri(match.Value.ToString());
                    packageDownloadLocation.Add(downloadUri);
                }
            }

            return packageDownloadLocation;
        }

        public WebSearch(PackageSource source)
        {
            _source = source;
        }

        public List<Uri> Search(string name)
        {
            // TODO: get top 3 links to download pages
            // using google search api/bing search api/website search box
            //return new List<Uri> { new Uri("https://notepad-plus-plus.org/download/"), new Uri("http://download.cnet.com/Notepad/3000-2352_4-10327521.html"), new Uri("http://notepad-plus.en.softonic.com/download"), new Uri("http://filehippo.com/download_notepad/") };

            return this.GetUrlIHtml("http://google.com/search?q=" + name + "%20download%20location", "/download", "google");
        }
    }
}
