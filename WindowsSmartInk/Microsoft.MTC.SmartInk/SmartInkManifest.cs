using System;
using System.Collections.Generic;

namespace Micosoft.MTC.SmartInk.Package
{
    public class SmartInkManifest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public DateTimeOffset DatePublished{ get; set; }
        public Dictionary<string,string> IconMap { get; private set; } = new Dictionary<string, string>();

        internal SmartInkManifest()
        {

        }
    }
}