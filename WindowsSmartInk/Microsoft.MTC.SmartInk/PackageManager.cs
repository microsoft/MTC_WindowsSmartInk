using Micosoft.MTC.SmartInk.Package.Storage;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package
{

    public class DownloadProgressEventArgs : EventArgs
    {
        public double Percentage { get; set; }
        public double BytesDownloaded { get; set; }
        public double TotalBytes { get; set; }
    }

    public class PackageManager
    {
        CancellationTokenSource _cts = new CancellationTokenSource();
        private IPackageManagerStorageProvider _provider;

        public event EventHandler ModelDownloadStarted;
        public event EventHandler ModelDownloadCompleted;
        public event EventHandler ModelDownloadError;
        public event EventHandler<DownloadProgressEventArgs> ModelDownloadProgress;

        public PackageManager()
        {
            _provider = new LocalAppDataPackageManagerStorageProvider();
        }

        public PackageManager(IPackageManagerStorageProvider provider)
        {
            _provider = provider;
        }

        public Task<SmartInkPackage> OpenPackageAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");

            return _provider.GetPackageAsync(packageName);
        }

        public async Task<SmartInkPackage> CreatePackageAsync(string packageName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");
            var packageProvider = await _provider.CreatePackageProviderAsync(packageName, overwrite);
            
            var package = new SmartInkPackage(packageName, packageProvider);
            await package.SaveAsync();
            return package;
            
        }

        public Task DeletePackageAsync(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException($"{nameof(packageName)} cannot be null or empty.");

            return _provider.DeletePackageAsync(packageName);
        }

        public async Task<IList<SmartInkPackage>> GetInstalledPackagesAsync()
        {
            var result = new List<SmartInkPackage>();
            var packages =await _provider.GetInstalledPackagesAsync();
            foreach (var package in packages)
            {
                var p = await _provider.GetPackageAsync(package);
                if (p != null)
                    result.Add(p);
            }

            return result;
        }

        public async Task PublishPackageAsync(SmartInkPackage package, IStorageFile destination)
        {
            if (package == null)
                throw new ArgumentNullException($"{nameof(package)} cannot be null");


            if (destination == null)
                throw new ArgumentNullException($"{nameof(destination)} cannot be null");


            Debug.WriteLine($"Publish package");
            var builder = new PackageBuilder();
            

        }
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
