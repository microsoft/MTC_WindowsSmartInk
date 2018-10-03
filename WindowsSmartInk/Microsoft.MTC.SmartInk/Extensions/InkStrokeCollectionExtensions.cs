/*
 * Copyright(c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.MTC.SmartInk.Extensions
{
    public static class InkStrokeCollectionExtensions
    {
        private static int DEFAULT_WIDTH = 256;
        private static int DEFAULT_HEIGHT = 256;

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

            return _boundingBox;
        }

        public static SoftwareBitmap DrawInk(this IList<InkStroke> strokes, double targetWidth = 0, double targetHeight = 0, float rotation = 0, Color? backgroundColor = null  )
        {
            if (strokes == null)
                throw new ArgumentNullException($"{nameof(strokes)} cannot be null");

            var boundingBox = strokes.GetBoundingBox();

            if (targetWidth == 0)
                targetWidth = DEFAULT_WIDTH;

            if (targetHeight == 0)
                targetHeight = DEFAULT_HEIGHT;

            if (backgroundColor == null)
                backgroundColor = Colors.White;

            var scale = CalculateScale(boundingBox, targetWidth, targetHeight);

            var scaledStrokes = ScaleAndTransformStrokes(strokes, scale,rotation);

            WriteableBitmap writeableBitmap = null;
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            using (CanvasRenderTarget offscreen = new CanvasRenderTarget(device, (float) targetWidth, (float)targetHeight, 96))
            {
                using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                {

                    ds.Units = CanvasUnits.Pixels;
                    ds.Clear(backgroundColor.Value);
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
        private static IList<InkStroke> ScaleAndTransformStrokes(IList<InkStroke> strokeList, float scale, float rotation = 0)
        {
            var builder = new InkStrokeBuilder();
            IList<InkStroke> rotatedStrokes;
            IList<InkStroke> translatedStrokes = new List<InkStroke>();
            var rotationMatrix = Matrix3x2.CreateRotation(ConvertDegreesToRadians(rotation));
            var scaleMatrix = Matrix3x2.CreateScale(scale);

            if (rotation != 0)
            {
                rotatedStrokes = new List<InkStroke>();
                foreach (var stroke in strokeList)
                {
                    var newStroke = builder.CreateStrokeFromInkPoints(stroke.GetInkPoints(), rotationMatrix);
                    rotatedStrokes.Add(newStroke);
                }
            }
            else
                rotatedStrokes = strokeList;

            var boundingBox = (rotation != 0) ? rotatedStrokes.GetBoundingBox() : strokeList.GetBoundingBox(); ;
            var translateMatrix = Matrix3x2.CreateTranslation((float)-boundingBox.X, (float)-boundingBox.Y);
           
            foreach (var stroke in rotatedStrokes)
            {
                
                var newStroke = builder.CreateStrokeFromInkPoints(stroke.GetInkPoints(), rotationMatrix * translateMatrix * scaleMatrix);
                translatedStrokes.Add(newStroke);
            }

            return translatedStrokes;
        }

        public static float ConvertDegreesToRadians(float degrees)
        {
            var radians = (Math.PI / 180) * degrees;
            return (float)radians;
        }
    }
}
