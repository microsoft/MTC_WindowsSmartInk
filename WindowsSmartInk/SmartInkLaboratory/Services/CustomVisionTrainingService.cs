
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

using Microsoft.Cognitive.CustomVision.Training.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public class CustomVisionTrainingService : CustomVisionClassifierBaseService, ITrainingService
    {
        public async Task<IList<Iteration>> GetTrainedIterationsAysnc()
        {
            try
            {
                var result = await _trainingApi.GetIterationsWithHttpMessagesAsync(_currentProject.Id);
                return (from i in result.Body select i).Where(i => i.TrainedAt != null).OrderByDescending(i => i.TrainedAt).ToList();
            }
            catch (Exception)
            {

                return null;
            }

            
        }

        public  Task<Iteration> TrainCurrentIterationAsync()
        {
            return Task.Run(async () => {
                try
                {
                    HttpOperationResponse<Iteration> result;
                    var response = await _trainingApi.TrainProjectWithHttpMessagesAsync(_currentProject.Id);
                    do
                    {
                        result = await _trainingApi.GetIterationWithHttpMessagesAsync(_currentProject.Id,response.Body.Id);
                        await Task.Delay(1000);
                    } while (result.Body.Status.ToLower() == "training");
                    return result.Body;
                }
                catch (HttpOperationException ex)
                {
                    var response = JsonConvert.DeserializeObject<TrainingServiceResponse>(ex.Response.Content);
                    throw new TrainingServiceException { Response = response };
                }
                catch (Exception ex)
                {

                    throw;
                }
            });
        }

        public async Task<Uri> GetModuleDownloadUriAsync(Guid iterationId)
        {
            Export export;
            try
            {
                var exports = (await _trainingApi.GetExportsWithHttpMessagesAsync(_currentProject.Id, iterationId)).Body;
                if (exports.Count == 0)
                    export = (await _trainingApi.ExportIterationWithHttpMessagesAsync(_currentProject.Id, iterationId, "onnx")).Body;
                else
                    export = exports[0];

                var uri = new Uri(export.DownloadUri );
                return uri;
            }
            catch (Exception ex)
            {

                return null;
            }
        
        }
          
            
            
        
    }
}
