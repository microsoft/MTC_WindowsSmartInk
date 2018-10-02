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
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class OpenPackageViewModel :ViewModelBase
    {
        PackageManager _manager = new PackageManager();
        private IProjectPackageMapper _mapper;
        private ITagService _tags;
        private IAppStateService _state;

        private List<ISmartInkPackage> _installed = new List<ISmartInkPackage>();
        public ObservableCollection<ISmartInkPackage> Packages { get; set; } = new ObservableCollection<ISmartInkPackage>();
          
        public RelayCommand<ISmartInkPackage> SelectPackage { get; set; }

        public OpenPackageViewModel(IProjectPackageMapper mapper, ITagService tags, IAppStateService state)
        {
            _mapper = mapper;
            _tags = tags;
            _state = state;

            _state.ProjectChanged += async (s,e) =>
            {
                await GetPackagesAsync();
            };

            this.SelectPackage = new RelayCommand<ISmartInkPackage>(async(package) => {
                if (package == null)
                    return;

                if (_state.CurrentPackage?.Name == package.Name)
                    return;

                var tagList = await _tags.GetTagsAsync();
                var updateTags = new Dictionary<Guid, string>();
                foreach (var tag in tagList)
                    updateTags.Add(tag.Id, tag.Name);
               
                await package.UpdateTagsAsync(updateTags);

                _state.SetCurrentPackage( package);


            });
        }

        public async Task GetPackagesAsync()
        {
            var packages = await _mapper.GetPackagesByProjectAsync(_state.CurrentProject.Id.ToString());
            _installed.Clear();
            _installed = (await _manager.GetLocalPackagesAsync()).ToList();
            var matches = from p in packages
                          join i in _installed on p.ToLower() equals i.Name.ToLower()
                          select i;

            LoadPackages(matches);
        }

        private void LoadPackages(IEnumerable<ISmartInkPackage> packages)
        {
            Packages.Clear();
            foreach (var p in packages)
                Packages.Add(p);
        }
    }
}
