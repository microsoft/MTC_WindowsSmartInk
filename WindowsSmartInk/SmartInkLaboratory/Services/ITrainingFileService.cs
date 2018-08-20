using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartInkLaboratory.Services
{
    public interface ITrainingFileService
    {
        Task<List<StorageFile>> CreateImageFileListAysnc();
         Task SaveBitmapAsync(WriteableBitmap cropped);
    }
}
