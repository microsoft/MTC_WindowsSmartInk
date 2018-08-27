using Microsoft.MTC.SmartInk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartInkLaboratory.Services
{
    public class InkModelManager : IInkModelManager
    {
        private InkModel _inkModel;

        public InkModelManager()
        {

        }

        public async Task OpenModel(string path, Dictionary<string,float> tags)
        {
            if (_inkModel == null)
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///AI/azureicons.onnx"));
                _inkModel = await InkModel.CreateModel(file,tags);
            }
        }

        public Task<ModelOutput> EvaluateAsync(ModelInput input)
        {
            if (input == null)
                throw new ArgumentNullException($"{nameof(input)} cannot be null or empty");

            return _inkModel.EvaluateAsync(input);
        }

    }
}
