using SmartInkLaboratory.Services;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

                var package = await _packageManager.CreatePackageAsync(Name);
                var taglist = await _tags.GetTagsAsync();
                var newTags = new Dictionary<Guid, string>();
                foreach (var tag in taglist)
                {
                    newTags.Add(tag.Id, tag.Name);
                }

                await package.AddTagsAsync(newTags);

                await _mapper.AddAsync(package.Name, _state.CurrentProject.Id.ToString());

                _state.CurrentPackage = package;

                Reset();
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
