/*
 * Copyright(c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using SmartInkLaboratory.Services;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Views.Dialogs;

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

        public SmartInkPackageViewModel CurrentPackage
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
                    await _packageManager.PublishPackageAsync(_state.CurrentPackage.BasePackage, file);
            },
                ()=> {
                    return _state.CurrentPackage != null; });
        }
    }
}
