using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    internal class Selector
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> Classes { get; set; }
        public Selector father { get; set; }
        public Selector child { get; set; }
        public Selector()
        {
            Classes = new List<string>();
        }

    }
}
