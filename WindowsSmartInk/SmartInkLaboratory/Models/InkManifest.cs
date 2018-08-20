using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Models
{
    public class InkManifest
    {
        public Guid ProjectId { get; set; }
        public Dictionary<Guid,string> IconMap { get; set; }
        public Dictionary<string,float> Tags { get; set; }
    }
}
