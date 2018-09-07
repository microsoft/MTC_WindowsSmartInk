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
 
using Microsoft.Cognitive.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<IDictionary<string,float >> GetPredictionAsync(Stream stream, Guid projectId, Guid iterationId)
        {
            if (stream == null)
                throw new ArgumentNullException($"{nameof(stream)} cannot be null");

            if (projectId == Guid.Empty || iterationId == Guid.Empty)
                throw new ArgumentException($"{nameof(projectId)} and/or {nameof(iterationId)} cannot be empty");

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
