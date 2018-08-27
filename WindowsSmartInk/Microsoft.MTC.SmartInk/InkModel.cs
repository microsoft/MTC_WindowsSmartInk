using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1115394e-f1eb-4398-a55f-14ade85ae44e

namespace Microsoft.MTC.SmartInk
{
    public sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput(Dictionary<string, float> tags)
        {
            this.classLabel = new List<string>();
            this.loss = tags;
            //new Dictionary<string, float>()
            //{
            //    { "aad", float.NaN },
            //    { "api_app", float.NaN },
            //    { "api_mgmt", float.NaN },
            //    { "asa", float.NaN },
            //    { "bot_services", float.NaN },
            //    { "function", float.NaN },
            //    { "key_vault", float.NaN },
            //    { "service_fabric", float.NaN },
            //};
        }
    }

    public sealed class InkModel
    {
        private LearningModelPreview learningModel;
        private Dictionary<string, float> _tags;

        public InkModel(Dictionary<string,float> tags)
        {
            if (tags == null || tags.Keys.Count == 0)
                throw new InvalidOperationException($"{nameof(tags)} cannot be null or empty.");
            _tags = tags;
        }

 

        public static async Task<InkModel> CreateModel(StorageFile file, Dictionary<string, float> tags)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            InkModel model = new InkModel(tags);
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
