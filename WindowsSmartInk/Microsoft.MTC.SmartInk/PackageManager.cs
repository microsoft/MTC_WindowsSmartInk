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
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package
{
    /// <summary>
    /// Provides status of ONNX model download from Custom Vision service
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public double Percentage { get; set; }
        public double BytesDownloaded { get; set; }
        public double TotalBytes { get; set; }
    }

    /// <summary>
    /// PackageManager handles creation of Smart Ink Packages, package lookup and 
    /// retrieval of ONNX models used by packages.  Also creates Nuget packages for 
    /// distribution of Smart Ink Packages.
    /// </summary>
    public class PackageManager
    {
        CancellationTokenSource _cts = new CancellationTokenSource();
        private IPackageManagerStorageProvider _provider;

        public event EventHandler ModelDownloadStarted;
        public event EventHandler ModelDownloadCompleted;
        public event EventHandler ModelDownloadError;
        public event EventHandler<DownloadProgressEventArgs> ModelDownloadProgress;

        /// <summary>
        /// Create new instance of PackageManager using the LocalAppDataPackageManagerStorageProvider
        /// </summary>
        public PackageManager()
        {
            _provider = new LocalAppDataPackageManagerStorageProvider();
        }

        /// <summary>
        /// Create a new instance of PackageManager using the provided instance of <c>IPackageManagerStorageProvider</c>
        /// </summary>
        /// <param name="provider">New instance of <see cref="IPackageManagerStorageProvider"/> of a storage provider</param>
        public PackageManager(IPackageManagerStorageProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Opens existing package
        /// </summary>
        /// <param name="packageName">Name of package</param>
        /// <returns>Exisiting smart ink package <see cref="SmartInkMediaPackage"/>. Returns null if package does not exist.</returns>
        public Task<ISmartInkPackage> OpenPackageAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");

            return _provider.GetPackageAsync(packageName);
        }

        /// <summary>
        /// Creates a new Smart Ink Package
        /// </summary>
        /// <param name="packageName">Name of new package</param>
        /// <param name="overwrite">Overwrites exisitng package.</param>
        /// <returns>New smart ink package <see cref="SmartInkMediaPackage"/>.</returns>
        //public async Task<SmartInkMediaPackage> CreateSmartInkMediaPackage(string packageName, int imagesize = 0, bool overwrite = false)
        //{
        //    if (string.IsNullOrWhiteSpace(packageName))
        //        throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");
        //    var packageProvider = await _provider.CreatePackageProviderAsync(packageName, overwrite);
            
        //    var package = new SmartInkMediaPackage(packageName, packageProvider, imagesize);
        //    await package.SaveAsync();
        //    return package;
        //}

        //public async Task<SmartInkPackage> CreateSmartInkPackage(string packageName, int imagesize = 0, bool overwrite = false)
        //{
        //    if (string.IsNullOrWhiteSpace(packageName))
        //        throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");
        //    var packageProvider = await _provider.CreatePackageProviderAsync(packageName, overwrite);

        //    var package = new SmartInkPackage(packageName, packageProvider, imagesize);
        //    await package.SaveAsync();
        //    return package;

        //}

        public async Task<T> CreatePackageAsync<T>(string packageName, int imagesize = 0, bool overwrite = false) where T : ISmartInkPackage
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            CultureInfo culture = null; // use InvariantCulture or other if you prefer

            var packageProvider = await _provider.CreatePackageProviderAsync(packageName, overwrite);

            object[] args = { packageName, packageProvider, imagesize };
            var package = (T) Activator.CreateInstance(typeof(T),args);
            await package.SaveAsync();
            return package;
        }
        /// <summary>
        /// Deletes existing package
        /// </summary>
        /// <param name="packageName">Name of package</param>
        public Task DeletePackageAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");

            return _provider.DeletePackageAsync(packageName);
        }

        /// <summary>
        /// Gets a list of all installed packaged using the current <c>IPackageManagerStorageProvider</c>
        /// </summary>
        /// <returns><c>IList</c> of SmartInk Packages</returns>
        public async Task<IList<ISmartInkPackage>> GetInstalledPackagesAsync()
        {
            var result = new List<ISmartInkPackage>();
            var packages =await _provider.GetInstalledPackagesAsync();
            foreach (var package in packages)
            {
                var p = await _provider.GetPackageAsync(package);
                if (p != null)
                    result.Add(p);
            }

            return result;
        }

        /// <summary>
        /// Creates a Nuget package from a SmartInk Package
        /// </summary>
        /// <param name="package">Package to use for Nuget creation</param>
        /// <param name="destination">File handle for saving the Nuget package</param>
        public async Task PublishPackageAsync(ISmartInkPackage package, IStorageFile destination)
        {
            if (package == null)
                throw new ArgumentNullException($"{nameof(package)} cannot be null");


            if (destination == null)
                throw new ArgumentNullException($"{nameof(destination)} cannot be null");


            Debug.WriteLine($"Publish package");
            ManifestMetadata metadata = new ManifestMetadata()
            {
                Title=$"{package.Name}",
                Owners = new string[] {"jabj"},
                Authors  =new string[] { "mauvo" },
                Version = new NuGet.Versioning.NuGetVersion(new Version(package.Version)),
                Id = $"SmartInk.{package.Name}",
                Description = $"{package.Description}",
                MinClientVersionString= "3.3.0"
            };

            NuGet.Frameworks.NuGetFramework targetFramework = new NuGet.Frameworks.NuGetFramework(new NuGet.Frameworks.NuGetFramework(".NETStandard2.0"));
            IEnumerable <NuGet.Packaging.Core.PackageDependency> packages = new NuGet.Packaging.Core.PackageDependency[] { new NuGet.Packaging.Core.PackageDependency("NETStandard.Library", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion(2, 0, 0))) };
            var dependencyGroup = new PackageDependencyGroup(targetFramework, packages);
            metadata.DependencyGroups = new PackageDependencyGroup[] { dependencyGroup };
            PackageBuilder builder = new PackageBuilder();
         
            builder.Populate(metadata);

            builder.ContentFiles.Add(new ManifestContentFiles() { Include = $"**/SmartInkPackages/*", CopyToOutput = "true", Flatten = "false", BuildAction="None" });
            builder.ContentFiles.Add(new ManifestContentFiles() { Include = $"**/SmartInkPackages/{package.Name}/*", CopyToOutput = "true", Flatten = "false", BuildAction = "None" });
            builder.ContentFiles.Add(new ManifestContentFiles() { Include = $"**/SmartInkPackages/{package.Name}/Model/*", CopyToOutput = "true", Flatten = "false" , BuildAction="None"});
            builder.ContentFiles.Add(new ManifestContentFiles() { Include = $"**/SmartInkPackages/{package.Name}/Icons/*", CopyToOutput = "true", Flatten = "false", BuildAction = "None" });

            var root = $"{_provider.RootFolderPath}\\{package.Name}\\";

            List<ManifestFile> manifestFiles = new List<ManifestFile>();

            var rootFolder = await StorageFolder.GetFolderFromPathAsync(root);
          
            
            var files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            foreach (var file in files)
            {
                
                ManifestFile manifestFile = new ManifestFile();
                manifestFile.Source = file.Path;
                manifestFile.Target = $"contentFiles\\cs\\uap10.0\\SmartInkPackages{file.Path.Replace(_provider.RootFolderPath, "")}";
                manifestFiles.Add(manifestFile);

            }

            builder.PopulateFiles(root, manifestFiles);


            using (var fileaccess = await destination.OpenAsync(FileAccessMode.ReadWrite))
            {
                Debug.WriteLine($"Publish package: {builder.ToString()}");
                var stream = fileaccess.AsStreamForWrite();
                builder.Save(stream);
                await stream.FlushAsync();
            }
            

        }

        /// <summary>
        /// Download ONNX model from the uri provided
        /// </summary>
        /// <param name="modelUri">Uri to the ONNX model</param>
        /// <returns><c>IStorageFile</c> of the downloaded ONNX file. <see cref="IStorageFile"/></returns>
        public async Task<IStorageFile> DownloadModelAsync(Uri modelUri)
        {
            if (modelUri == null)
                throw new ArgumentNullException($"{nameof(modelUri)} canot be null");

            var filename = System.IO.Path.GetFileName(modelUri.LocalPath);

            var tempFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            BackgroundDownloader downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(modelUri, tempFile);

            await HandleDownloadAsync(download, true);
            ModelDownloadCompleted?.Invoke(this, null);

            return tempFile;

        }

        /// <summary>
        /// Handle the Background download operation
        /// </summary>
        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {

            try
            {
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    ModelDownloadStarted?.Invoke(this, null);
                    await download.StartAsync().AsTask(_cts.Token, progressCallback);
                }
                else
                {
                    await download.AttachAsync().AsTask(_cts.Token, progressCallback);
                }

                var response = download.GetResponseInformation();

            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                ModelDownloadError?.Invoke(this, null);
            }

        }
        /// <summary>
        /// Progress of the background download
        /// </summary>
        /// <param name="operation"></param>
        private void DownloadProgress(DownloadOperation operation)
        {
            var currentProgress = operation.Progress;
            if (currentProgress.TotalBytesToReceive > 0)
            {
                if (ModelDownloadProgress != null)
                {
                    var progressEvent = new DownloadProgressEventArgs();
                    progressEvent.Percentage = currentProgress.BytesReceived * 100 / currentProgress.TotalBytesToReceive;
                    progressEvent.TotalBytes = currentProgress.TotalBytesToReceive;
                    progressEvent.BytesDownloaded = currentProgress.BytesReceived;
                    ModelDownloadProgress.Invoke(this, progressEvent);
                }
            }
        }

    }
}
