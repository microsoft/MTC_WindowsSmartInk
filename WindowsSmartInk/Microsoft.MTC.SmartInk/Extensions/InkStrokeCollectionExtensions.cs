using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace Microsoft.MTC.SmartInk.Extensions
{
    public static class InkStrokeCollectionExtensions
    {
        public static Rect GetBoundingBox(this IList<InkStroke> strokes)
        {
            Rect _boundingBox = Rect.Empty;
            foreach (var stroke in strokes)
            {
                if (_boundingBox == Rect.Empty)
                    _boundingBox = new Rect(stroke.BoundingRect.X, stroke.BoundingRect.Y, stroke.BoundingRect.Width, stroke.BoundingRect.Height);
                else
                    _boundingBox = _boundingBox.CombineWith(stroke.BoundingRect);
            }

            var drawAttributes = strokes[0].DrawingAttributes;
            var strokeSize = drawAttributes.Size;

            //add border buffer for stroke width
            //_boundingBox.Height += strokeSize.Height * 2;
            //_boundingBox.Width += strokeSize.Width * 2;

            return _boundingBox;
        }
    }
}
