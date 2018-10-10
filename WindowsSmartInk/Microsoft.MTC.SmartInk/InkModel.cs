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
        public TensorString classLabel; // shape(-1,1)
        public IList<Dictionary<string, float>> loss;
    }

    internal sealed class SmartInkModel
    {
        //private Dictionary<string, float> _tags = new Dictionary<string, float>();
        private LearningModel _model;
        private LearningModelSession _session;
        private LearningModelBinding _binding;
        public static async Task<SmartInkModel> CreateModelAsync(IStorageFile file, IList<string> tags)
        {
            //var tempDir = ApplicationData.Current.LocalCacheFolder;
            //var modelfile = await file.CopyAsync(tempDir,file.Name,NameCollisionOption.ReplaceExisting);
            try
            {
                //LearningModel learningModel = await LearningModel.LoadFromStorageFileAsync(file);
                //SmartInkModel model = new SmartInkModel();

                ////foreach (var t in tags)
                ////    model._tags.Add(t, float.NaN);

                //model._learningModel = learningModel;
                //model._learningSession = new LearningModelSession(model._learningModel);
                //model._learningBinding = new LearningModelBinding(model._learningSession);
                //return model;
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
        public async Task<SmartInkModelOutput> EvaluateAsync(SoftwareBitmap bitmap)
        {
            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            var imageFeatureValue = ImageFeatureValue.CreateFromVideoFrame(videoFrame);
            var input = new SmartInkModelInput() { data = imageFeatureValue };

            _binding.Bind("data", input.data);
            LearningModelEvaluationResult result = await _session.EvaluateAsync(_binding,"0");
            var keys = result.Outputs.Keys.ToList();
            var values = result.Outputs.Values.ToList();
            var output = new SmartInkModelOutput();
            output.classLabel = result.Outputs["classLabel"] as TensorString;
            
            output.loss = result.Outputs["loss"] as IList<Dictionary<string, float>>;
            return output;
        }
    }
}
