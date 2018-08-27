using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MTC.SmartInk;

namespace SmartInkLaboratory.Services
{
    public interface IInkModelManager
    {
        Task<ModelOutput> EvaluateAsync(ModelInput input);
        Task OpenModel(string path, Dictionary<string, float> tags);
    }
}