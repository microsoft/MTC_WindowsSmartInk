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
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;

namespace SmartInkLaboratory.ViewModels
{
    public class PackageManagerViewModel : ViewModelBase, IVisualState
    {
        private PackageManager _packageManager = new PackageManager();
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

        public PackageManagerViewModel(ITagService tags, IProjectPackageMapper mapper, IAppStateService state, IDialogService dialog )
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
                    await _state.CurrentPackage.RemoveTagAsync(e.DeletedTag.Id);
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
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Nuget Package", new List<string>() { ".nupkg" });
                var nugetFileName = $"SmartInk.{_state.CurrentPackage.Name}.{_state.CurrentPackage.Version}.nupkg";
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = nugetFileName;
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                    await _packageManager.PublishPackageAsync(_state.CurrentPackage, file);
            },
                ()=> {
                    return _state.CurrentPackage != null; });
        }
    }
}
