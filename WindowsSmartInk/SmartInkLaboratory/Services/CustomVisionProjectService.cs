using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public class CustomVisionProjectService : CustomVisionClassifierBaseService, IProjectService
    {
      
        private IList<Project> _projects;

        

        public CustomVisionProjectService() : base()
        {
            
            base.ResourceKeysChanged += async (s, e) => {
                _projects = await GetProjectsAsync(true);
                var pick = (from p in _projects select p).OrderByDescending(p => p.Name).FirstOrDefault();
                OpenProject(pick);
            };

        }

        //public new void SetCurrentProject(Project project)
        //{
        //    base.SetCurrentProject(project);
        //}

        public void OpenProject(Project project)
        {
            base.SetCurrentProject( project);
        }

        public Task<Project> CreateProjectAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty");
            return _trainingApi.CreateProjectAsync(name, description);
        }

        public async Task<IList<Project>> GetProjectsAsync(bool refresh = false)
        {
            if (string.IsNullOrWhiteSpace(_trainingApi.ApiKey))
                throw new InvalidOperationException("Service has not been initialized with training api key");
            if (_projects == null || refresh)
             _projects = await _trainingApi.GetProjectsAsync();

            return _projects;
        }

       public  Task DeleteProjectAsync(Guid projectId)
        {
            return _trainingApi.DeleteProjectAsync(projectId);
        }
    }
}
