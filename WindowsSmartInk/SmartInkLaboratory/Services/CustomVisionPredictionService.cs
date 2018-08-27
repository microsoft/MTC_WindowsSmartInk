using Microsoft.Cognitive.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public class CustomVisionPredictionService : IPredictionService
    {
        static PredictionEndpoint _endpoint;
        private Guid _currentProject;

      

        public void Initialize(string predictionKey)
        {
            if (string.IsNullOrWhiteSpace(predictionKey))
                throw new ArgumentNullException($"{nameof(predictionKey)} cannot be null or empty");
            _endpoint = new PredictionEndpoint() { ApiKey = predictionKey };
            
        }

        public async Task<IDictionary<string,float >> GetPrediction(Stream stream, Guid projectId, Guid? iterationId = null)
        {
            var predictions = new Dictionary<string, float>();
            try
            {

                
                var result = await _endpoint.PredictImageAsync(projectId, stream, iterationId);
                var tags = (from p in result.Predictions select new { p.Tag, p.Probability }).OrderByDescending(p => p.Probability);
                
                foreach (var t in tags)
                {
                    predictions.Add(t.Tag, (float) t.Probability);
                        
                }
                return predictions;
            }
            catch (Exception ex)
            {
                return predictions;
            }
        }
    }
}
