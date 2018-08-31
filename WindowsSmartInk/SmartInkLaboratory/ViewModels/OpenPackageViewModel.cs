using SmartInkLaboratory.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class OpenPackageViewModel :ViewModelBase
    {
        PackageManager _manager = new PackageManager();
        private IProjectPackageMapper _mapper;
        private ITagService _tags;
        private IAppStateService _state;

        private List<SmartInkPackage> _installed = new List<SmartInkPackage>();
        public ObservableCollection<SmartInkPackage> Packages { get; set; } = new ObservableCollection<SmartInkPackage>();
          
        public RelayCommand<SmartInkPackage> SelectPackage { get; set; }

        public OpenPackageViewModel(IProjectPackageMapper mapper, ITagService tags, IAppStateService state)
        {
            _mapper = mapper;
            _tags = tags;
            _state = state;

            _state.ProjectChanged += async (s,e) =>
            {
                await GetPackagesAsync();
            };

            this.SelectPackage = new RelayCommand<SmartInkPackage>(async(package) => {
                if (package != null && _state.CurrentPackage?.Name == package.Name)
                    return;
                var tagList = await _tags.GetTagsAsync();
                var updateTags = new Dictionary<Guid, string>();
                foreach (var tag in tagList)
                    updateTags.Add(tag.Id, tag.Name);

                await package.UpdateTagsAsync(updateTags);
                _state.CurrentPackage = package;


            });
        }

        public async Task GetPackagesAsync()
        {
            var packages = await _mapper.GetPackagesByProjectAsync(_state.CurrentProject.Id.ToString());
            _installed.Clear();
            _installed = (await _manager.GetInstalledPackagesAsync()).ToList();
            var matches = from p in packages
                          join i in _installed on p.ToLower() equals i.Name.ToLower()
                          select i;

            LoadPackages(matches);
        }

        private void LoadPackages(IEnumerable<SmartInkPackage> packages)
        {
            Packages.Clear();
            foreach (var p in packages)
                Packages.Add(p);
        }
    }
}
