using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInkLaboratory.AI;

namespace SmartInkLaboratory.Services
{
    public interface IInkModelManager
    {
        Task<ModelOutput> EvaluateAsync(ModelInput input);
        Task OpenModel(string path, Dictionary<string, float> tags);
    }
}