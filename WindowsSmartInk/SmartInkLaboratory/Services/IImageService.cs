using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartInkLaboratory.Services
{
    public interface IImageService 
    {
        Task<bool> UploadImageAsync(IStorageFile imageFile, IList<string> tags = null);
    }
}
