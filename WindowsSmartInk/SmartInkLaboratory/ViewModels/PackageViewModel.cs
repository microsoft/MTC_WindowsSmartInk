using SmartInkLaboratory.Services;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Views.Dialogs;

namespace SmartInkLaboratory.ViewModels
{
    public class PackageViewModel : ViewModelBase, IVisualState
    {
        private PackageManager _manager = new PackageManager();
        private ITagService _tags;
        private IProjectPackageMapper _mapper;
        private IAppStateService _state;
        private IDialogService _dialog;
        private int _packageCount;

        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public SmartInkPackage CurrentPackage
        {
            get { return _state.CurrentPackage; }
            
        }

        public RelayCommand NewPackage { get;  private set; }
        public RelayCommand OpenPackage { get; private set; }
        public RelayCommand PublishPackage { get; private set; }

        public string CurrentVisualState => throw new NotImplementedException();

        public PackageViewModel(ITagService tags, IProjectPackageMapper mapper, IAppStateService state, IDialogService dialog )
        {
            _tags = tags;
            _mapper = mapper;
            _state = state;
            _dialog = dialog;

            _state.KeysChanged += (s, e) => {
                NewPackage.RaiseCanExecuteChanged();
            };

            _state.TagDeleted += async (s, e) => {
                if (_state.CurrentPackage != null)
                    await _state.CurrentPackage.RemoveTagAsync(e.DeletedTag.Name);
            };

            _state.PackageChanged += (s, e) => {
                RaisePropertyChanged(nameof(CurrentPackage));
                PublishPackage.RaiseCanExecuteChanged();
            };

            _state.ProjectChanged += async (s, e) => {
                _packageCount = ( await _mapper.GetPackagesByProjectAsync(_state.CurrentProject.Id.ToString())).Count;
                OpenPackage.RaiseCanExecuteChanged();
            };

            this.NewPackage = new RelayCommand(async () => {
                await _dialog.OpenAsync(DialogKeys.NewPackage);
            },
            () =>{ return _state.CurrentKeys != null;  });

            this.OpenPackage = new RelayCommand(async () => {
                await _dialog.OpenAsync(DialogKeys.OpenPackage);
            },
            ()=>{
                return _state.CurrentProject != null && _packageCount > 0;
            });

            this.PublishPackage = new RelayCommand(async() => {
                //await _manager.PublishPackageAsync(_state.CurrentPackage.Name);
            },
                ()=> {
                    return _state.CurrentPackage != null; });
        }
    }
}
