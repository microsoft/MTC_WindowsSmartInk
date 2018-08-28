using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;
using Windows.Graphics.Imaging;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1a567c32-c564-4965-9f65-8eabc7fcaf03

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
        private LearningModelPreview _learningModel;
        public static async Task<Model> CreateModelAsync(IStorageFile file, IList<string> tags)
        {


            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            Model model = new Model();

            foreach (var t in tags)
                model._tags.Add(t, float.NaN);

            model._learningModel = learningModel;
            return model;
        }
        public async Task<ModelOutput> EvaluateAsync(SoftwareBitmap bitmap)
        {
            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            var input = new ModelInput() { data = videoFrame };
            ModelOutput output = new ModelOutput();
            output.loss = _tags;

            LearningModelBindingPreview binding = new LearningModelBindingPreview(_learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await _learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
