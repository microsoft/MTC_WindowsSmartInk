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
using System.Linq;

namespace Microsoft.MTC.SmartInk
{
    internal sealed class SmartInkModelInput
    {
        public ImageFeatureValue data { get; set; }
    }

    internal sealed class SmartInkModelOutput
    {
        public TensorString ClassLabel = TensorString.Create(new long[] { 1, 1 });

        public IList<IDictionary<string, float>> Loss = new List<IDictionary<string, float>>();
    }

    internal sealed class SmartInkModel
    {
        private LearningModel _model;
        private LearningModelSession _session;
        private LearningModelBinding _binding;
        public static async Task<SmartInkModel> CreateModelAsync(IStorageFile file, IList<string> tags)
        {
            try
            {
                SmartInkModel learningModel = new SmartInkModel();
                learningModel._model =  await LearningModel.LoadFromStorageFileAsync(file);
                learningModel._session = new LearningModelSession(learningModel._model);
                learningModel._binding = new LearningModelBinding(learningModel._session);
                return learningModel;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<IDictionary<string,float>> EvaluateAsync(SoftwareBitmap bitmap)
        {
            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            var imageFeatureValue = ImageFeatureValue.CreateFromVideoFrame(videoFrame);
            var input = new SmartInkModelInput() { data = imageFeatureValue };
            var output = new SmartInkModelOutput();
            _binding.Bind("data", input.data);

            _binding.Bind("classLabel", output.ClassLabel);

            _binding.Bind("loss", output.Loss);
            LearningModelEvaluationResult result = await _session.EvaluateAsync(_binding,"0");
            
            output.ClassLabel = result.Outputs["classLabel"] as TensorString;//).GetAsVectorView()[0];
            
            output.Loss = result.Outputs["loss"] as IList<IDictionary<string, float>>;
            var dict = new Dictionary<string, float>();
            foreach (var key in output.Loss[0].Keys)
            {
                dict.Add(key, output.Loss[0][key]);
            }

            return dict;
        }
    }
}
