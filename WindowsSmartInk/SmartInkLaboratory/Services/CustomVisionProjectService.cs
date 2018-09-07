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
 
 using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return _trainingApi.CreateProjectAsync(name, description, domainId: Guid.Parse("0732100f-1a38-4e49-a514-c9b44c697ab5"));
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
