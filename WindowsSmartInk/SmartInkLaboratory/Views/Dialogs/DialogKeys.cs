using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Views.Dialogs
{
  
    public sealed class DialogKeys
    {
        public static string ResourceList { get; private set; } = nameof(ResourceList);
        public static string NewPackage { get; internal set; } = nameof(NewPackage);
        public static string OpenPackage { get; internal set; } = nameof(OpenPackage);
        public static string ManageProjects { get; internal set; } = nameof(ManageProjects);
    }
    
}
