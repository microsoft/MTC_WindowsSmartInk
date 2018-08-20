using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartInkLaboratory.Services
{
    public class CustomVisionImageService : CustomVisionClassifierBaseService, IImageService
    {
        public CustomVisionImageService() : base()
        {

        }

        public async Task<bool> UploadImageAsync(IStorageFile imageFile, IList<string> tags = null)
        {
            var stream = await imageFile.OpenReadAsync();
            //var tagIds = (from t in tags select t.Id.ToString()).ToList();
            var result =    await _trainingApi.CreateImagesFromDataWithHttpMessagesAsync(_currentProject.Id, stream.AsStream(), tags);
            return true;
        }
    }
}
