using SmartInkLaboratory.Services.Platform;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public interface IClassifierService
    {
        void SetCurrentProject(Project project);
        void SetKeys(ResourceKeys trainingKey);
    }
}
