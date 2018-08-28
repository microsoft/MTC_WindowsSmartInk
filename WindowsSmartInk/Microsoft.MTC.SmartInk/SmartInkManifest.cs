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
        public Dictionary<Guid,string> IconMap { get; private set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> TagList { get; private set; } = new Dictionary<Guid, string>();
        public string Model { get; set; }

        internal SmartInkManifest()
        {

        }

      
    }
}