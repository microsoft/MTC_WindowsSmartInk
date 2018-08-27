using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1a567c32-c564-4965-9f65-8eabc7fcaf03

namespace SmartInkLaboratory
{
    public sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput()
        {
            this.classLabel = new List<string>();
            //this.loss = new Dictionary<string, float>()
            //{
            //    { "aad", float.NaN },
            //    { "api_app", float.NaN },
            //    { "api_mgmt", float.NaN },
            //    { "asa", float.NaN },
            //    { "bot_services", float.NaN },
            //    { "cosmos_db", float.NaN },
            //    { "event_hub", float.NaN },
            //    { "function", float.NaN },
            //    { "iot_hub", float.NaN },
            //    { "key_vault", float.NaN },
            //    { "other", float.NaN },
            //    { "service_fabric", float.NaN },
            //    { "sql", float.NaN },
            //    { "web_app", float.NaN },
            //};
        }
    }

    public sealed class Model
    {
        public Dictionary<string, float> TagLabels = new Dictionary<string, float>();
        private LearningModelPreview learningModel;
        public static async Task<Model> CreateModel(StorageFile file, IList<string> tags)
        {
           
                
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            Model model = new Model();

            foreach (var t in tags)
                model.TagLabels.Add(t, float.NaN);

            model.learningModel = learningModel;
            return model;
        }
        public async Task<ModelOutput> EvaluateAsync(ModelInput input) {
            ModelOutput output = new ModelOutput();
            output.loss = TagLabels;
                
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
