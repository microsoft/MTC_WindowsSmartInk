using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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

        public static SoftwareBitmap DrawInk(this IList<InkStroke> strokes, double width = 0, double height = 0)
        {
            if (strokes == null)
                throw new ArgumentNullException($"{nameof(strokes)} cannot be null");

            var boundingBox = strokes.GetBoundingBox();

            if (width == 0)
                width = boundingBox.Width;

            if (height == 0)
                height = boundingBox.Height;

            var scale = CalculateScale(boundingBox, width, height);

            var scaledStrokes = ScaleAndTransformStrokes(strokes, scale);


            WriteableBitmap writeableBitmap = null;
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            using (CanvasRenderTarget offscreen = new CanvasRenderTarget(device, (float) width, (float)height, 96))
            {
                using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                {

                    ds.Units = CanvasUnits.Pixels;
                    ds.Clear(Colors.White);
                    ds.DrawInk(scaledStrokes);
                }

                writeableBitmap = new WriteableBitmap((int)offscreen.SizeInPixels.Width, (int)offscreen.SizeInPixels.Height);
                offscreen.GetPixelBytes().CopyTo(writeableBitmap.PixelBuffer);
            }

            SoftwareBitmap inkBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                 writeableBitmap.PixelBuffer,
                 BitmapPixelFormat.Bgra8,
                 writeableBitmap.PixelWidth,
                 writeableBitmap.PixelHeight,
                 BitmapAlphaMode.Premultiplied
             );

            return inkBitmap;
        }

        private static float CalculateScale(Rect boundingBox, double width, double height)
        {
            var wScale = (float)(width/ boundingBox.Width);
            var hScale = (float)(height/ boundingBox.Height);

            return (wScale <= hScale) ? wScale : hScale;
        }

        private static Vector2 CalculateOffset(IList<InkStroke> strokes)
        {
            double inkSize = 0;
            var boundingBox = strokes.GetBoundingBox();
            foreach (var stroke in strokes)
            {
                var drawingAttributes = stroke.DrawingAttributes;
                var strokeSize = drawingAttributes.Size;
                if (strokeSize.Height > inkSize)
                    inkSize = strokeSize.Height;
                if (strokeSize.Width > inkSize)
                    inkSize = strokeSize.Width;

            }

            var point = new Vector2();
            point.X = (float)(-boundingBox.Left + inkSize);
            point.Y = (float)(-boundingBox.Top + inkSize);
            return point;
        }
        private static IList<InkStroke> ScaleAndTransformStrokes(IList<InkStroke> strokeList, float scale)
        {
            var builder = new InkStrokeBuilder();
            var newStrokeList = new List<InkStroke>();
            var boundingBox = strokeList.GetBoundingBox();

            foreach (var singleStroke in strokeList)
            {
                var translateMatrix = new Matrix(1, 0, 0, 1, -boundingBox.X, -boundingBox.Y);

                var newInkPoints = new List<InkPoint>();
                var originalInkPoints = singleStroke.GetInkPoints();
                foreach (var point in originalInkPoints)
                {
                    var newPosition = translateMatrix.Transform(point.Position);
                    var newInkPoint = new InkPoint(newPosition, point.Pressure, point.TiltX, point.TiltY, point.Timestamp);
                    newInkPoints.Add(newInkPoint);
                }

                var newStroke = builder.CreateStrokeFromInkPoints(newInkPoints, new Matrix3x2(scale, 0, 0, scale, 0, 0));

                newStrokeList.Add(newStroke);
            }

            return newStrokeList;
        }


    }
}
