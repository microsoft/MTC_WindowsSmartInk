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

using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package.Storage
{
    public interface IPackageStorageProvider
    {
        Task SaveIconAsync(IStorageFile file);
        Task<IStorageFile> GetIconAsync(string tag);
        Task DeleteIconAsync(string icon);
        Task<IStorageFile> GetModelAsync(string filename);
        Task SaveModelAsync(IStorageFile model);
        Task SaveManifestAsync(SmartInkManifest manifest);
        Task<SmartInkManifest> GetManifestAsync();
    }

    public interface IPackageManagerStorageProvider
    {
        string RootFolderPath { get; }
        Task<ISmartInkPackage> GetPackageAsync(IStorageFolder packageFolder);
        Task<IPackageStorageProvider> CreatePackageProviderAsync(string packageName, bool overwrite = false);
        Task  DeleteLocalPackageAsync(string packagename);
        Task<IList<IStorageFolder>> GetLocalPackagesAsync();
        Task<IList<IStorageFolder>> GetInstalledPackagesAsync();
    }
}