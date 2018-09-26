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

using Micosoft.MTC.SmartInk.Package.Storage;
using Microsoft.MTC.SmartInk;
using Microsoft.MTC.SmartInk.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Input.Inking;

namespace Micosoft.MTC.SmartInk.Package
{

    /// <summary>
    /// Manages metadata, model and icons of the Smart Ink Package.  
    /// New packages are created using <see cref="PackageManager.CreatePackageAsync(string, bool)"/>
    /// </summary>
    public class SmartInkPackage : ISmartInkPackage
    {
        private readonly int INK_IMAGE_SIZE = 256;

        protected SmartInkManifest _manifest;
        protected IPackageStorageProvider _provider;
        internal Model _model;

        public IReadOnlyList<string> Tags { get { return GetTags(); } }

        // These properties map to the properties set on the Nuget Package
        public string Name
        {
            get { return _manifest.Name; }
            set
            {
                if (_manifest.Name == value)
                    return;
                _manifest.Name = value;
            }
        }

        public string Description
        {
            get { return _manifest.Description; }
            set
            {
                if (_manifest.Description == value)
                    return;
                _manifest.Description = value;
            }
        }

        public string Version
        {
            get { return _manifest.Version; }
            set
            {
                if (_manifest.Version == value)
                    return;
                _manifest.Version = value;
            }
        }

        public string Author
        {
            get { return _manifest.Author; }
            set
            {
                if (_manifest.Author == value)
                    return;
                _manifest.Author = value;
            }
        }

        public DateTimeOffset DatePublished
        {
            get { return _manifest.DatePublished; }
            set
            {
                if (_manifest.DatePublished == value)
                    return;
                _manifest.DatePublished = value;
            }
        }


        public SoftwareBitmap LastEvaluatedBitmap { get; set; }
        public bool IsLocalModelAvailable => !string.IsNullOrWhiteSpace(_manifest.Model);

        internal SmartInkPackage(string name, IPackageStorageProvider provider, int imageSize = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty.");
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest = new SmartInkManifest() { Name = name };

            if (imageSize != 0)
                INK_IMAGE_SIZE = imageSize;

        }

        internal SmartInkPackage(SmartInkManifest manifest, IPackageStorageProvider provider, int imageSize = 0)
        {
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest = manifest ?? throw new ArgumentNullException($"{nameof(manifest)} cannot be null");

            if (imageSize != 0)
                INK_IMAGE_SIZE = imageSize;

        }

        /// <summary>
        /// Get all tags in the package
        /// </summary>
        /// <returns>List of tag names</returns>
        private IReadOnlyList<string> GetTags()
        {
            List<string> tags = new List<string>();
            foreach (var t in _manifest.TagList.Values)
                tags.Add(t);

            return tags.AsReadOnly();
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
        public async Task AddTagsAsync(Dictionary<Guid, string> tags)
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
        /// Evaluates image using package ONNX model
        /// </summary>
        /// <param name="bitmap">Image to be evaluated</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> containing scoring for submitted image (tag/probability)</returns>
        public async Task<IDictionary<string, float>> EvaluateAsync(SoftwareBitmap bitmap, float threshold = 0)
        {
            if (bitmap == null)
                throw new ArgumentNullException($"{nameof(bitmap)} cannot be null");

            if (string.IsNullOrWhiteSpace(_manifest.Model))
                throw new InvalidOperationException("Model file not available");

            IDictionary<string, float> result;

            var modelfile = await _provider.GetModelAsync(_manifest.Model);

            if (modelfile == null)
                throw new InvalidOperationException("Model file not found");

            _model = await Model.CreateModelAsync(modelfile, _manifest.TagList.Values.ToList());
            var output = await _model.EvaluateAsync(bitmap);
            if (threshold > 0)
                result = output.loss.Where(p => p.Value >= threshold).ToDictionary(p => p.Key, p => p.Value);
            else
                result = output.loss;

            LastEvaluatedBitmap = bitmap;
            return result.OrderByDescending(x => x.Value).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Evaluates ink strokes using package ONNX model
        /// </summary>
        /// <param name="strokes">List of strokes to be evaluated against model as single image</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> containing scoring for submitted image (tag/probability)</returns>
        public async Task<IDictionary<string, float>> EvaluateAsync(IList<InkStroke> strokes, float threshold = 0)
        {
            if (strokes == null)
                throw new ArgumentNullException($"{nameof(strokes)} cannot be null");

            if (strokes.Count == 0)
                return new Dictionary<string, float>();

            var bitmap = strokes.DrawInk(INK_IMAGE_SIZE, INK_IMAGE_SIZE);

            return await EvaluateAsync(bitmap, threshold);
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



    }
}
