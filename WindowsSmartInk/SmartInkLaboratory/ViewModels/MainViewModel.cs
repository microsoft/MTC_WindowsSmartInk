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
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.UI.Input.Inking;

namespace SmartInkLaboratory.ViewModels
{
    public class MainViewModel : AsyncViewModel, IInkProcessor
    {
        private IDialogService _dialog;
        private IAppStateService _state;

        public InteractionMode Mode { get; set; }
    
        public IconMapViewModel IconMap { get; set; }
      
        public TrainViewModel Train { get; set; }
        public TestViewModel Test { get; set; }



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






        public MainViewModel(IconMapViewModel iconMap,
                             TrainViewModel train,
                             TestViewModel test,
                             IAppStateService state,
                             IDialogService dialog)
        {
            IconMap = iconMap;
            Train = train;
            Test = test;
            _state = state;
            _dialog = dialog;

            _state.PackageChanged += (s, e) =>
            {
                HasPackage = _state.CurrentPackage != null;
            };
         
        }

        //public async Task<IDictionary<string , float >> ProcessInkImageAsync(SoftwareBitmap bitmap)
        //{
        //    switch (Mode)
        //    {
        //        case InteractionMode.Mapping:
        //            return null;
        //        case InteractionMode.Training:
        //            return await Train.ProcessInkImageAsync(bitmap);
        //        case InteractionMode.Testing:
        //            return  await Test.ProcessInkImageAsync(bitmap);
        //        case InteractionMode.Packaging:
        //            break;
        //    }

        //    return null;
        //}


        public async Task<IDictionary<string, float>> ProcessInkAsync(IList<InkStroke> strokes)
        {
            switch (Mode)
            {
                case InteractionMode.Mapping:
                    return null;
                case InteractionMode.Training:
                    return await Train.ProcessInkAsync(strokes);
                case InteractionMode.Testing:
                    return await Test.ProcessInkAsync(strokes);
                case InteractionMode.Packaging:
                    break;
            }

            return null;
        }




        //protected override async Task InitializeAsync()
        //{
        //    await ResourceKeys.Initialization;
        //}
    }

    
}