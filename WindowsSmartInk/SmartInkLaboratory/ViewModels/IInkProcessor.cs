using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartInkLaboratory.ViewModels
{
    public interface IInkProcessor
    {
        Task<IList<(string tag, double probability)>> ProcessInkImageAsync(WriteableBitmap bitmap);
    }
}
