using SmartInkLaboratory.Services.Platform;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public interface IAppStateService
    {
        event EventHandler KeysChanged;
        event EventHandler ProjectChanged;
        event EventHandler TagChanged;
        event EventHandler PackageChanged;
        event EventHandler IconChanged;

        ResourceKeys CurrentKeys{ get; set; }
        Project CurrentProject { get; set; }
        Tag CurrentTag { get; set; }
        SmartInkPackage CurrentPackage { get; set; }

        void IconUpdated();
    }
}
