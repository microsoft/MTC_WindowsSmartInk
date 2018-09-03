using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartInkLaboratory.ViewModels
{
    public interface IInkProcessor
    {
        Task<IDictionary<string, float>> ProcessInkAsync(IList<InkStroke> strokes);
    }
}
