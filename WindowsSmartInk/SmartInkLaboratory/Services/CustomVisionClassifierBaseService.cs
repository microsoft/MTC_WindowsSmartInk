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

using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using SmartInkLaboratory.Services.Platform;

using System;

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
            _trainingApi.BaseUri = new System.Uri("https://southcentralus.api.cognitive.microsoft.com/customvision/v2.2/Training");
            _currentProject = null;
            ResourceKeysChanged?.Invoke(this, null);
        }

       

        
    }
}
