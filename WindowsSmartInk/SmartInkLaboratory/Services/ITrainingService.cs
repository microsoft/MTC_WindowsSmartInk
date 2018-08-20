using Microsoft.Cognitive.CustomVision.Training.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public interface ITrainingService
    {
        Task<IList<Iteration>> GetIterationsAysnc();
        Task<Iteration> TrainAsync();
    }
}