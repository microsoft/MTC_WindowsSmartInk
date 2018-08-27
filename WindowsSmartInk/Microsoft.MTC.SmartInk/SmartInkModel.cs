using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;
using System.Linq;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1115394e-f1eb-4398-a55f-14ade85ae44e

namespace Micosoft.MTC.SmartInk
{
    internal sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    internal sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput(Dictionary<string, float> tags)
        {
            this.classLabel = new List<string>();
            this.loss = tags;
           
        }
    }

    internal sealed class SmartInkModel
    {
        private LearningModelPreview learningModel;
        private Dictionary<string, float> _tags;

        private SmartInkModel(Dictionary<string, float> tags)
        {
            if (tags == null || tags.Keys.Count == 0)
                throw new InvalidOperationException($"{nameof(tags)} cannot be null or empty.");
            _tags = tags;
        }



        public static async Task<SmartInkModel> CreateModel(IStorageFile file, IEnumerable<string> tags)
        {
            if (tags == null || tags.Count() == 0)
                throw new InvalidOperationException($"{nameof(tags)} cannot be null or empty.");

            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            var tagDictionary = new Dictionary<string, float>();
            foreach (var tag in tags)
                tagDictionary.Add(tag, float.NaN);

            SmartInkModel model = new SmartInkModel(tagDictionary);
            model.learningModel = learningModel;
            return model;
        }
        public async Task<ModelOutput> EvaluateAsync(ModelInput input) {
            ModelOutput output = new ModelOutput(_tags);

            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
