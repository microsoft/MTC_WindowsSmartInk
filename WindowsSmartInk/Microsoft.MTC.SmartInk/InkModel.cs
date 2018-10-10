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
 
 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.AI.MachineLearning;

namespace Microsoft.MTC.SmartInk
{
    internal sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    internal sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput()
        {
            this.classLabel = new List<string>();
        }
    }

    internal sealed class Model
    {
        private Dictionary<string, float> _tags = new Dictionary<string, float>();
        private LearningModel _learningModel;
        public static async Task<Model> CreateModelAsync(IStorageFile file, IList<string> tags)
        {
            //var tempDir = ApplicationData.Current.LocalCacheFolder;
            //var modelfile = await file.CopyAsync(tempDir,file.Name,NameCollisionOption.ReplaceExisting);
            try
            {
                LearningModel learningModel = await LearningModel.LoadFromStorageFileAsync(file);
                Model model = new Model();

                foreach (var t in tags)
                    model._tags.Add(t, float.NaN);

                model._learningModel = learningModel;
                return model;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<ModelOutput> EvaluateAsync(SoftwareBitmap bitmap)
        {
            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            var input = new ModelInput() { data = videoFrame };
            ModelOutput output = new ModelOutput();
            output.loss = _tags;

            var session = new LearningModelSession(_learningModel);
            LearningModelBinding binding = new LearningModelBinding(session);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResult evalResult = await session.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
