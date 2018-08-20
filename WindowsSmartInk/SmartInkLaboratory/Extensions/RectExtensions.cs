using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SmartInkLaboratory.Extensions
{
    public static class RectExtensions
    {
        public static Rect CombineWith(this Rect r, Rect rect)
        {
            var top = (r.Top < rect.Top) ? r.Top : rect.Top;
            var left = (r.Left < rect.Left) ? r.Left : rect.Left;
            var bottom = (r.Bottom < rect.Bottom) ? rect.Bottom : r.Bottom;
            var right = (r.Right < rect.Right) ? rect.Right : r.Right;

            var newRect = new Rect(new Point(left, top), new Point(right, bottom));
            return newRect;
        }
    }
}
