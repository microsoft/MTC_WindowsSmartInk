using SmartInkLaboratory.Services;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class PackageViewModel : ViewModelBase, IVisualState
    {
        private ITagService _tags;
        private IAppStateService _state;
        private SmartInkPackage _currentPackage;

        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public SmartInkPackage CurrentPackage
        {
            get { return _currentPackage; }
            set
            {
                if (_currentPackage == value)
                    return;
                _currentPackage = value;
                RaisePropertyChanged(nameof(CurrentPackage));
            }
        }

        public string CurrentVisualState => throw new NotImplementedException();

        public PackageViewModel(ITagService tags, IAppStateService state )
        {
            _tags = tags;
            _state = state;

            _state.PackageChanged += (s, e) => {
                if (_state.CurrentPackage == null)
                {
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "NoPackage" });
                }
                else
                {
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "HasPackage" });
                }
            };
        }
    }
}
