using SmartInkLaboratory.Services.Platform;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public  class CustomVisionClassifierBaseService : IClassifierService
    {
        protected event EventHandler ResourceKeysChanged;
        protected event EventHandler ProjectChanged;

        protected static TrainingApi _trainingApi;
        protected static Project _currentProject;
        protected static ResourceKeys _currentKeys;

        public CustomVisionClassifierBaseService()
        {
           
        }

        public void SetCurrentProject(Project project)
        {
            _currentProject = project;
            ProjectChanged?.Invoke(this, null);
        }

        public void SetKeys(ResourceKeys keys)
        {
          
            if (string.IsNullOrWhiteSpace(keys.TrainingKey))
                throw new ArgumentNullException($"{nameof(keys.TrainingKey)} cannot be null or empty");
            _currentKeys = keys;
            _trainingApi = new TrainingApi() { ApiKey = _currentKeys.TrainingKey };
            _currentProject = null;
            ResourceKeysChanged?.Invoke(this, null);
        }

       

        
    }
}
