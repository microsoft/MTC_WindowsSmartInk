using Micosoft.MTC.SmartInk.Package.Storage;
using Microsoft.Graphics.Canvas;
using Microsoft.MTC.SmartInk;
using Microsoft.MTC.SmartInk.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Micosoft.MTC.SmartInk.Package
{
   
    /// <summary>
    /// Manages metadata, model and icons of the Smart Ink Package.  
    /// New packages are created using <see cref="PackageManager.CreatePackageAsync(string, bool)"/>
    /// </summary>
    public class SmartInkPackage
    {
        private const int INK_IMAGE_SIZE = 256;
  
        private SmartInkManifest _manifest;
        private IPackageStorageProvider _provider;
        private Model _model;

        // These properties map to the properties set on the Nuget Package
        public string Name { get { return _manifest.Name; } }
        public string Description { get; set; } = "Windows 10 SmartInk Package";
        public string Version { get; set; } = "1.0.0.0";
        public string Author { get; set; }
        public DateTimeOffset DatePublished { get; set; }

        public SoftwareBitmap LastEvaluatedBitmap { get; set; }

        internal SmartInkPackage(string name,  IPackageStorageProvider provider)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty.");
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest =  new SmartInkManifest() { Name = name };
           
        }

        internal SmartInkPackage(SmartInkManifest manifest,IPackageStorageProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest = manifest ?? throw new ArgumentNullException($"{nameof(manifest)} cannot be null");
           
        }

        /// <summary>
        /// Get all tags in the package
        /// </summary>
        /// <returns>List of tag names</returns>
        public IList<string> GetTags()
        {
            List<string> tags = new List<string>();
            foreach (var t in _manifest.TagList.Values)
                tags.Add(t);

            return tags;
        }

        /// <summary>
        /// Updates package tag list for all matching tags.  Tags not present in package are ignored.
        /// </summary>
        /// <param name="tags">Dictionary of tag Id (key) and tag name (value)</param>
        /// <returns></returns>
        public async Task UpdateTagsAsync(Dictionary<Guid, string> tags)
        {
            if (tags == null || tags.Count == 0)
                return;

            bool isDirty = false;
            foreach (var tag in tags)
            {
                if (_manifest.TagList.ContainsKey(tag.Key) && _manifest.TagList[tag.Key] != tag.Value)
                {
                    _manifest.TagList[tag.Key] = tag.Value;
                    isDirty = true;
                }

            }

            if (isDirty)
                await SaveAsync();
        }

        /// <summary>
        /// Add new tags to the package
        /// </summary>
        /// <param name="tags"><see cref="Dictionary{TKey, TValue}"/> of tag Id (key) and tag name (value)</param>
        /// <returns></returns>
        public async Task AddTagsAsync(Dictionary<Guid,string> tags)
        {
            if (tags == null || tags.Count == 0)
                return;

            foreach (var tag in tags)
            {
                if (!_manifest.TagList.ContainsKey(tag.Key))
                    _manifest.TagList.Add(tag.Key, tag.Value);
                else
                    _manifest.TagList[tag.Key] = tag.Value;
            }

            await SaveAsync();
        }

        /// <summary>
        /// Adds a new tag to the package
        /// </summary>
        /// <param name="tagId"><see cref="Guid"/>Tag Id</param>
        /// <param name="tagName">Name of tag</param>
        /// <returns></returns>
        public async Task AddTagAsync(Guid tagId, string tagName)
        {
            if (tagId == null || tagId == Guid.Empty)
                throw new ArgumentException($"{nameof(tagId)} cannot be null or empty");

            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentNullException($"{nameof(tagName)} cannot be null");

            if (!_manifest.IconMap.ContainsKey(tagId))
                _manifest.TagList.Add(tagId, tagName);
             else   
                _manifest.TagList[tagId] = tagName;

            await SaveAsync();
        }

        /// <summary>
        /// Removes tag from package and associated icon
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public async Task RemoveTagAsync(Guid tagId)
        {
            if (tagId == null || tagId == Guid.Empty)
                return;

            if (!_manifest.TagList.ContainsKey(tagId))
                return;

            _manifest.TagList.Remove(tagId);

            if (_manifest.IconMap.ContainsKey(tagId))
            {
                var icon = _manifest.IconMap[tagId];
                _manifest.IconMap.Remove(tagId);
                if (icon != null)
                    await _provider.DeleteIconAsync(icon);
            }
        }

        /// <summary>
        /// Associate icon with tag and store in package
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task SaveIconAsync(Guid tagId, IStorageFile file)
        {
            if (tagId == null || tagId == Guid.Empty)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty.");

            if (file == null)
                throw new ArgumentNullException($"{nameof(file)} canot be null.");

            if (!_manifest.TagList.ContainsKey(tagId))
                throw new InvalidOperationException($"Tag:{nameof(tagId)} does not exist");

            if (_manifest.IconMap.ContainsKey(tagId))
            {
                await _provider.DeleteIconAsync(_manifest.IconMap[tagId]);
                _manifest.IconMap[tagId] = file.Name;
            }
            else
                _manifest.IconMap.Add(tagId, file.Name);

            await _provider.SaveIconAsync( file);
        }


        /// <summary>
        /// /// Associate icon with tag and store in package. <seealso cref="SaveIconAsync(Guid, IStorageFile)"/>
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task SaveIconAsync(string tagName, IStorageFile file)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentNullException($"{nameof(tagName)} cannot be null or empty.");

            var tag = (from v in _manifest.TagList where v.Value == tagName select v.Key).FirstOrDefault();
            if (tag != null)
                await SaveIconAsync(tag, file);
            else
                throw new InvalidOperationException($"Tag:{tagName} does not exist");
        }

        /// <summary>
        /// Retrieves an icon for a given tag <seealso cref="GetIconAsync(Guid)"/>
        /// </summary>
        /// <param name="tag"></param>
        /// <returns><see cref="IStorageFile" />handle to icon file</returns>
        public Task<IStorageFile> GetIconAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");

            Guid tagId;
            if (Guid.TryParse(tag, out tagId))
                return GetIconAsync(tagId);

            throw new ArgumentException($"{nameof(tag)}:{tag} is not a valid guid");
        }

        /// <summary>
        /// Retrieves an icon for a given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns><see cref="IStorageFile" />handle to icon file</returns>
        public async Task<IStorageFile> GetIconAsync(Guid tagId)
        {
            if (tagId == null || Guid.Empty == tagId)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty");

            if (!_manifest.IconMap.ContainsKey(tagId))
                return null;

            return await _provider.GetIconAsync(_manifest.IconMap[tagId]);
        }

      
        /// <summary>
        /// Evaluates image using package ONNX model
        /// </summary>
        /// <param name="bitmap">Image to be evaluated</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> containing scoring for submitted image (tag/probability)</returns>
        public async Task<IDictionary<string, float>> EvaluateAsync(SoftwareBitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException($"{nameof(bitmap)} cannot be null");

            if (string.IsNullOrWhiteSpace(_manifest.Model))
                throw new InvalidOperationException("Model file not available");

            var modelfile = await _provider.GetModelAsync(_manifest.Model);

            if (modelfile == null)
                throw new InvalidOperationException("Model file not found");

            _model = await Model.CreateModelAsync(modelfile, _manifest.TagList.Values.ToList());
            var result = await _model.EvaluateAsync(bitmap);
            LastEvaluatedBitmap = bitmap;
            return result.loss;
        }

        /// <summary>
        /// Evaluates ink strokes using package ONNX model
        /// </summary>
        /// <param name="strokes">List of strokes to be evaluated against model as single image</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> containing scoring for submitted image (tag/probability)</returns>
        public async Task<IDictionary<string, float>> EvaluateAsync(IList<InkStroke> strokes)
        {
            if (strokes == null)
                throw new ArgumentNullException($"{nameof(strokes)} cannot be null");

            if (strokes.Count == 0)
                return new Dictionary<string, float>();

            var bitmap = DrawInk(strokes);

            return await EvaluateAsync(bitmap);
        }

     
        public SoftwareBitmap DrawInk(IList<InkStroke> strokes)
        {
            if (strokes == null)
                throw new ArgumentNullException($"{nameof(strokes)} cannot be null");

            var boundingBox = strokes.GetBoundingBox();
            var scale = CalculateScale(boundingBox);

            var scaledStrokes = ScaleAndTransformStrokes(strokes, scale);


            WriteableBitmap writeableBitmap = null;
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            using (CanvasRenderTarget offscreen = new CanvasRenderTarget(device, INK_IMAGE_SIZE, INK_IMAGE_SIZE, 96))
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

        private float CalculateScale(Rect boundingBox)
        {
            var wScale = (float)(INK_IMAGE_SIZE / boundingBox.Width);
            var hScale = (float)(INK_IMAGE_SIZE / boundingBox.Height);

            return (wScale <= hScale) ? wScale : hScale; 
        }

        private Vector2 CalculateOffset(IList<InkStroke> strokes)
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

        /// <summary>
        /// Save the current package state
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            await _provider.SaveManifestAsync(_manifest);
        }

        /// <summary>
        /// Save the model file to the package
        /// </summary>
        /// <param name="modelFile"><see cref="IStorageFile"/> handle to Onnx file</param>
        /// <returns></returns>
        public async Task SaveModelAsync(IStorageFile modelFile)
        {
            if (modelFile.FileType != ".onnx")
                throw new InvalidOperationException($"{modelFile} must have a file type of .onnx");
           
            await _provider.SaveModelAsync(modelFile);
            _manifest.Model = modelFile.Name;
            await SaveAsync();
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
