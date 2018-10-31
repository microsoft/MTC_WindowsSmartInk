using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace EBC_InkDemo.Converters
{
    public class InkDelayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var delay = (int)value;
            var outputValue = ((float) delay / 1000);
            var output = outputValue.ToString("F", CultureInfo.InvariantCulture);
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
