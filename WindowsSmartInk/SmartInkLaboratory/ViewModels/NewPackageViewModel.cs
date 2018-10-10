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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace SmartInkLaboratory.ViewModels
{
    public class NewPackageViewModel : ViewModelBase
    {
        private IProjectPackageMapper _mapper;
        private ITagService _tags;
        private IAppStateService _state;
        private INavigationService _nav;
        private Project _currentProject;
        private PackageManager _packageManager;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;

                _name = value;
                Save.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(Name));
            }
        }

      

      


        public string Description { get; set; }
        public string Author { get; set; }
        private string _version = "1.0.0.0";
        public string Version
        {
            get { return _version; }
            set
            {
                if (_version == value)
                    return;

                _version = value;
                Save.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(Version));
                
            }
        }




        private bool _isMediaPackage = true;
        public bool IsMediaPackage
        {
            get { return _isMediaPackage; }
            set
            {
                if (_isMediaPackage == value)
                    return;
                _isMediaPackage = value;
                RaisePropertyChanged(nameof(IsMediaPackage));
            }
        }



        public RelayCommand Save { get; set; }
     

        public NewPackageViewModel(IProjectPackageMapper mapper, ITagService tags, IAppStateService state,INavigationService nav)
        {
            _mapper = mapper;
            _tags = tags;
            _state = state;
            _nav = nav;
            _packageManager = new PackageManager();
            
            this.Save = new RelayCommand(async () =>
            {
                ISmartInkPackage package;
                try
                {
                    if (IsMediaPackage)
                        package = await _packageManager.CreateLocalPackageAsync<SmartInkMediaPackage>(Name);
                    else
                        package = await _packageManager.CreateLocalPackageAsync<SmartInkPackage>(Name);

                    var taglist = await _tags.GetTagsAsync();
                    var newTags = new Dictionary<Guid, string>();
                    foreach (var tag in taglist)
                    {
                        newTags.Add(tag.Id, tag.Name);
                    }

                    await package.AddTagsAsync(newTags);

                    await _mapper.AddAsync(package.Name, _state.CurrentProject.Id.ToString());

                    _state.SetCurrentPackage(package);

                    Reset();
                }
                catch (Exception)
                {
                   
                }
            },
            ()=> {
                return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Version);
            });
         
        }

        private void Reset()
        {
            Name = Description = Author = Version = string.Empty;
        }

        //protected override async Task InitializeAsync()
        //{
        //    await Keys.Initialization;
        //    await Projects.Initialization;
        //}
    }
}
