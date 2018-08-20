using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartInkLaboratory.Services.Platform;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Cognitive.CustomVision.Training.Models;

namespace SmartInkLaboratory.Services
{
    public class AppStateService : IAppStateService
    {
        static ResourceKeys _currentKeys;
        static Project _currentProject;
        static SmartInkPackage _currentPackage;
        static Tag _currentTag;
        static IClassifierService _classifier;

        public ResourceKeys CurrentKeys {
            get => _currentKeys;
            set {
                _currentKeys = value;
                _classifier.SetKeys(_currentKeys);
                KeysChanged?.Invoke(this, null);
            }
        }
        public Project CurrentProject {
            get => _currentProject;
            set
            {
                _currentProject = value;
                _classifier.SetCurrentProject(_currentProject);
                ProjectChanged?.Invoke(this, null);
            }
        }

        public Tag CurrentTag
        {
            get => _currentTag;
            set
            {
                _currentTag = value;
                TagChanged?.Invoke(this, null);
            }
        }
        public SmartInkPackage CurrentPackage {
            get => _currentPackage;
            set
            {
                _currentPackage = value;
                PackageChanged?.Invoke(this,null);
            }
        }

        public event EventHandler KeysChanged;
        public event EventHandler ProjectChanged;
        public event EventHandler TagChanged;
        public event EventHandler PackageChanged;
        public event EventHandler IconChanged;

        public AppStateService(IClassifierService classifier)
        {
            if (_classifier == null)
                _classifier = classifier;
            else
            {
                classifier.SetKeys(CurrentKeys);
                classifier.SetCurrentProject(CurrentProject);
                _classifier = classifier;
            }
        }

        public void IconUpdated()
        {
            IconChanged?.Invoke(this, null);
        }
    }
}
