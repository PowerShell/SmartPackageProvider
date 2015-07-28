using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartProvider
{
    static class DownloadLinkFinder
    {
        internal async static Task<Uri> GetDownloadLink(Uri url)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();



            return new Uri("https://notepad-plus-plus.org/repository/6.x/6.8/npp.6.8.Installer.exe");
        }
    }
}
