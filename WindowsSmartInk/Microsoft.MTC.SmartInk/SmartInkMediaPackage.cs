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
    public class SmartInkMediaPackage : SmartInkPackage, IMediaPackage
    {

        public SmartInkMediaPackage() : base()
        {

        }
  
        public SmartInkMediaPackage(string name,  IPackageStorageProvider provider, int imagesize = 0) : base(name, provider, imagesize)
        {  
        }

        public SmartInkMediaPackage(SmartInkManifest manifest,IPackageStorageProvider provider, int imagesize = 0) :base(manifest, provider, imagesize)
        {  
        }
        
        /// <summary>
        /// Associate icon with tag and store in package
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task SaveMediaAsync(Guid tagId, IStorageFile file)
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
        /// /// Associate icon with tag and store in package. <seealso cref="SaveMediaAsync(Guid, IStorageFile)"/>
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task SaveMediaAsync(string tagName, IStorageFile file)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentNullException($"{nameof(tagName)} cannot be null or empty.");

            var tag = (from v in _manifest.TagList where v.Value == tagName select v.Key).FirstOrDefault();
            if (tag != null)
                await SaveMediaAsync(tag, file);
            else
                throw new InvalidOperationException($"Tag:{tagName} does not exist");
        }

        /// <summary>
        /// Retrieves an icon for a given tag name
        /// </summary>
        /// <param name="tagname">tag name</param>
        /// <returns><see cref="IStorageFile" />handle to icon file</returns>
        public Task<IStorageFile> GetMediaByNameAsync(string tagname)
        {
            var match = (from t in _manifest.TagList where t.Value == tagname.ToLower() select t.Key).FirstOrDefault();
            if (match == null)
                return null;
            return GetMediaAsync(match);
        }

        /// <summary>
        /// Retrieves an icon for a given tag <seealso cref="GetMediaAsync(Guid)"/>
        /// </summary>
        /// <param name="tag"></param>
        /// <returns><see cref="IStorageFile" />handle to icon file</returns>
        public Task<IStorageFile> GetMediaAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");

            Guid tagId;
            if (Guid.TryParse(tag, out tagId))
                return GetMediaAsync(tagId);

            throw new ArgumentException($"{nameof(tag)}:{tag} is not a valid guid");
        }

        /// <summary>
        /// Retrieves an icon for a given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns><see cref="IStorageFile" />handle to icon file</returns>
        public async Task<IStorageFile> GetMediaAsync(Guid tagId)
        {
            if (tagId == null || Guid.Empty == tagId)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty");

            if (!_manifest.IconMap.ContainsKey(tagId))
                return null;

            return await _provider.GetIconAsync(_manifest.IconMap[tagId]);
        }

        public override Task SaveAsync()
        {
            _manifest.IsMediaPackage = true;
            return base.SaveAsync();
        }

    }
}
