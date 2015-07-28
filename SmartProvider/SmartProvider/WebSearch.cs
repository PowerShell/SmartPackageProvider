using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartProvider
{
    class WebSearch
    {
        private PackageSource _source;

        public WebSearch(PackageSource source)
        {
            _source = source;
        }

        public List<Uri> Search(string name)
        {
            // TODO: get top 3 links to download pages
            // using google search api/bing search api/website search box
            return new List<Uri> { new Uri("https://notepad-plus-plus.org/download/"), new Uri("http://download.cnet.com/Notepad/3000-2352_4-10327521.html"), new Uri("http://notepad-plus.en.softonic.com/download") };
        }
    }
}
