using SmartInkLaboratory.Services;
using AMP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cognitive.CustomVision.Training.Models;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;
using Newtonsoft.Json;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Services.Platform;
using GalaSoft.MvvmLight;

namespace SmartInkLaboratory.ViewModels
{
    public class ProjectChangedEventArgs : EventArgs
    {
        public Project NewProject { get; set; }
    }

    public class ProjectsViewModel : ViewModelBase
    {
        private IProjectService _projects;
        private IAppStateService _state;

        public ObservableCollection<Project> ProjectsList { get; private set; } = new ObservableCollection<Project>();

      
        public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public RelayCommand<Project> SelectProject { get; set; }
        public RelayCommand<string> CreateProject { get; set; }

     
        public Project CurrentProject
        {
            get { return _state.CurrentProject; }
            set
            {
              
                _state.CurrentProject = value;
                ProjectChanged?.Invoke(this, new ProjectChangedEventArgs { NewProject = _state.CurrentProject });
                _projects.OpenProject(_state.CurrentProject);
              
                RaisePropertyChanged(nameof(CurrentProject));
            }
        }


        public ProjectsViewModel(IProjectService projects, IAppStateService state)
        {
            _projects = projects;
            _state = state;



            _state.KeysChanged += async (s,e) =>
            {
                await LoadAsync();
            };

            this.SelectProject = new RelayCommand<Project>((project) => {
                if (project == null)
                    return;

                _state.CurrentProject = project;
                _state.CurrentPackage = null;
                ApplicationData.Current.LocalSettings.Values["LastProject"] = _state.CurrentProject.Name;
            });

            this.CreateProject = new RelayCommand<string>(async(project) => {
                await _projects.CreateProjectAsync(project);
            });
        }

        public async Task LoadAsync()
        {
            ProjectsList.Clear();
            var projects = await _projects.GetProjectsAsync(true);
            foreach (var p in projects)
            {
                ProjectsList.Add(p);
            }
            var last = ApplicationData.Current.LocalSettings.Values["LastProject"] as string;
            var found = (last != null) ? (from p in projects where p.Name == last select p).FirstOrDefault() : null;
            if (found == null)
                CurrentProject = (from p in projects select p).FirstOrDefault();
            else
                CurrentProject = found;
        }

       

    }
}
