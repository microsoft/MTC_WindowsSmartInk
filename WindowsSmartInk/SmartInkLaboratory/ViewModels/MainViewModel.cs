using System;
using System.Threading.Tasks;
using AMP.ViewModels;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using SmartInkLaboratory.Services.UX;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Cognitive.CustomVision.Training.Models;
using SmartInkLaboratory.Views.Dialogs;
using System.Diagnostics;
using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Micosoft.MTC.SmartInk.Package;

namespace SmartInkLaboratory.ViewModels
{
    public class MainViewModel : AsyncViewModel, IInkProcessor
    {
        private IDialogService _dialog;
        private IAppStateService _state;

        public InteractionMode Mode { get; set; }
        public PackageManagerViewModel Manager { get; private set; }
        public ImageTagsViewModel ImageTags { get; set; }
        public IconMapViewModel IconMap { get; set; }
        public ProjectsViewModel Projects { get; set; }
        public ResourceKeysViewModel ResourceKeys { get; set; }
        public TrainViewModel Train { get; set; }
        public TestViewModel Test { get; set; }
        public PackageViewModel Package { get; set; }

        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                if (_isReady == value)
                    return;
                _isReady = value;
                RaisePropertyChanged(nameof(IsReady));
            }
        }

        private bool _hasPackage;
        public bool HasPackage
        {
            get { return _hasPackage; }
            set
            {
                if (_hasPackage == value)
                    return;
                _hasPackage = value;
                RaisePropertyChanged(nameof(HasPackage));
            }
        }


   

        public RelayCommand NewPackage { get; set; }
        public RelayCommand OpenPackage { get; set; }

        public MainViewModel(ImageTagsViewModel imageTags,
                             IconMapViewModel iconMap,
                             ProjectsViewModel projects,
                             ResourceKeysViewModel resourceKeys,
                             TrainViewModel train,
                             TestViewModel test,
                             PackageViewModel package,
                          
                             IAppStateService state,
                             IDialogService dialog)
        {
            ImageTags = imageTags;
            IconMap = iconMap;
            Projects = projects;
            ResourceKeys = resourceKeys;
            Train = train;
            Test = test;
            Package = package;
            _state = state;
            _dialog = dialog;

            IsReady = ResourceKeys.CurrentKeys != null;

            _state.KeysChanged += (s, e) => {
               IsReady = true;
            };


            

            _state.PackageChanged += (s, e) => {
                HasPackage = _state.CurrentPackage != null;
            };
         

         
        }

        public async Task<(string tag, double probability)> ProcessInkImageAsync(WriteableBitmap bitmap)
        {
            switch (Mode)
            {
                case InteractionMode.Mapping:
                    return (null,0);
                case InteractionMode.Training:
                    return await Train.ProcessInkImageAsync(bitmap);
                case InteractionMode.Testing:
                    return  await Test.ProcessInkImageAsync(bitmap);
                case InteractionMode.Packaging:
                    break;
            }

            return (null, 0);
        }

       

        //protected override async Task InitializeAsync()
        //{
        //    await ResourceKeys.Initialization;
        //}
    }

    
}