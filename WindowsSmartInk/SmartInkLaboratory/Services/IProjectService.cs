using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Cognitive.CustomVision.Training.Models;

namespace SmartInkLaboratory.Services
{
    public interface IProjectService 
    {
        void OpenProject(Project project);
        Task<IList<Project>> GetProjectsAsync(bool refresh = false);
        Task<Project> CreateProjectAsync(string name, string description = null);
        Task DeleteProjectAsync(Guid projectId);
    }
}