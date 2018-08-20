using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public interface IPredictionService
    {
        Task<IList<(string tag, double probability)>> GetPrediction(Stream stream, Guid projectId, Guid? iterationId = null);
        void Initialize(string predictionKey);
    }
}