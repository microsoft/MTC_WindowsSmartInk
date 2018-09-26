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

using GalaSoft.MvvmLight;
using Micosoft.MTC.SmartInk.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input.Inking;

namespace SmartInkLaboratory.ViewModels
{
    public class SmartInkPackageViewModel : ViewModelBase
    {
        public ISmartInkPackage BasePackage { get; private set; }


        public bool IsMediaPackage { get { return BasePackage is SmartInkMediaPackage; } }
        public string Name
        {
            get { return BasePackage.Name; }
            set
            {
                if (BasePackage.Name == value)
                    return;
                BasePackage.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        public string Version
        {
            get { return BasePackage.Version; }
            set
            {
                if (BasePackage.Version == value)
                    return;
                BasePackage.Version = value;
                RaisePropertyChanged(nameof(Version));
            }
        }



        public IReadOnlyList<string> Tags  { get { return BasePackage.Tags; } }

        public bool IsLocalModelAvailable
        {
            get { return BasePackage.IsLocalModelAvailable; }
            
        }


        public SmartInkPackageViewModel(ISmartInkPackage package)
        {
            BasePackage = package;
        }

        public Task AddTagAsync(Guid tagId, string tagName)
        {
            return BasePackage.AddTagAsync(tagId, tagName);
        }

        public Task SaveAsync()
        {
            return BasePackage.SaveAsync();
        }

        public async Task<IStorageFile> GetMediaAsync(Guid tagId)
        {
            if (IsMediaPackage)
                return await ((SmartInkMediaPackage)BasePackage).GetMediaAsync(tagId);
            else
                return null;
        }
        public async Task SaveMediaAsync(Guid tagId, IStorageFile file)
        {
            if (IsMediaPackage)
                await ((SmartInkMediaPackage)BasePackage).SaveMediaAsync(tagId, file);
        }

        public Task<IDictionary<string, float>> EvaluateAsync(IList<InkStroke> strokes, float threshold = 0)
        {
            return BasePackage.EvaluateAsync(strokes, threshold);
        }

        public Task RemoveTagAsync(Guid tagId)
        {
            return BasePackage.RemoveTagAsync(tagId);
        }

        public Task SaveModelAsync(IStorageFile modelFile)
        {
            return BasePackage.SaveModelAsync(modelFile);
        }

    }
}
