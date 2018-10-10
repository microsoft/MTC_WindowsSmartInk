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
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;
using SmartInkLaboratory.Services.UX;
using GalaSoft.MvvmLight;
using System.Diagnostics;
using SmartInkLaboratory.Views.Dialogs;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

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
        private IDialogService _dialog;

  
      
        public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public ObservableCollection<Project> ProjectsList { get; private set; } = new ObservableCollection<Project>();

        private bool _isCreating;
        public bool IsCreating
        {
            get { return _isCreating; }
            set
            {
                if (_isCreating == value)
                    return;
                _isCreating = value;
                RaisePropertyChanged(nameof(IsCreating));
            }
        }

        public RelayCommand ShowCreate { get; set; }
        public RelayCommand<Project> SelectProject { get; private set; }
        public RelayCommand<string> CreateProject { get; private set; }
        public RelayCommand ManageProjects { get; private set; }
        public RelayCommand<Project> DeleteProject { get; private set; }
     
        public Project CurrentProject
        {
            get { return _state.CurrentProject; }
            set
            {
                if (_state.CurrentProject?.Id == value.Id)
                    return;

                _state.CurrentProject = value;
                ProjectChanged?.Invoke(this, new ProjectChangedEventArgs { NewProject = _state.CurrentProject });
                if (_state.CurrentProject != null)
                    _projects.OpenProject(_state.CurrentProject);
              
                RaisePropertyChanged(nameof(CurrentProject));
            }
        }


        public ProjectsViewModel(IProjectService projects, IAppStateService state, IDialogService dialog)
        {
            _projects = projects;
            _state = state;
            _dialog = dialog;


            _state.KeysChanged += async (s,e) =>
            {
                await LoadAsync();
            };

            this.ShowCreate = new RelayCommand(() => { IsCreating = true; });

            this.SelectProject = new RelayCommand<Project>((project) => {
                if (project == null )
                    return;

                _state.CurrentProject = project;
                _state.SetCurrentPackage(null);
                ApplicationData.Current.LocalSettings.Values["LastProject"] = _state.CurrentProject.Name;
            });

            this.CreateProject = new RelayCommand<string>(async(project) => {
                if (string.IsNullOrWhiteSpace(project))
                    return;

                var newProject = await _projects.CreateProjectAsync(project);
                ProjectsList.Add(newProject);
                CurrentProject = newProject;
                IsCreating = false;
            });
            this.ManageProjects = new RelayCommand(async () => {
                await _dialog.OpenAsync(DialogKeys.ManageProjects,this);
            });
            this.DeleteProject = new RelayCommand<Project>(async (project) => {
                Debug.WriteLine($"Delete {project.Name}");
                var confirm = await _dialog.ConfirmAsync("Confirm", $"Are you sure you want to delete {project.Name}?", "Yes", "No");

                if (!confirm)
                    return;

                await _projects.DeleteProjectAsync(project.Id);
                if (CurrentProject.Id == project.Id)
                    CurrentProject = null;
                ProjectsList.Remove(project);
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
