using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1ca76d48-1136-42f9-8753-dea81d7654a1

namespace SmartInkLaboratory
{
    public sealed class _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "aad", float.NaN },
                { "api_app", float.NaN },
                { "api_mgmt", float.NaN },
                { "asa", float.NaN },
                { "bot_services", float.NaN },
                { "cosmos_db", float.NaN },
                { "event_hub", float.NaN },
                { "function", float.NaN },
                { "iot_hub", float.NaN },
                { "key_vault", float.NaN },
                { "other", float.NaN },
                { "service_fabric", float.NaN },
                { "sql", float.NaN },
                { "web_app", float.NaN },
            };
        }
    }

    public sealed class _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1Model
    {
        private LearningModelPreview learningModel;
        public static async Task<_x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1Model> Create_x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1Model(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1Model model = new _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1Model();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<_x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelOutput> EvaluateAsync(_x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelInput input) {
            _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelOutput output = new _x0034_b7e8535_x002D_9fcf_x002D_4162_x002D_aae1_x002D_af3f171e249b_1ca76d48_x002D_1136_x002D_42f9_x002D_8753_x002D_dea81d7654a1ModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
