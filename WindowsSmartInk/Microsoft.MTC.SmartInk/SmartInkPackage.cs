using Micosoft.MTC.SmartInk.Package.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Micosoft.MTC.SmartInk.Package
{
    public class DownloadProgressEventArgs: EventArgs
    {
        public double Percentage { get; set; }
        public double BytesDownloaded { get; set; }
        public double TotalBytes { get; set; }
    }

    public class SmartInkPackage
    {
        bool _isDirty = true;
        CancellationTokenSource _cts;

        private SmartInkManifest _manifest;
        private IPackageStorageProvider _provider;

        public event EventHandler ModelDownloadStarted;
        public event EventHandler ModelDownloadCompleted;
        public event EventHandler ModelDownloadError;
        public event EventHandler<DownloadProgressEventArgs> ModelDownloadProgress;

        public string Name { get { return _manifest.Name; } }
        public string Description { get { return _manifest.Description; } set { _manifest.Description = value; _isDirty = true; } }
        public string Version { get { return _manifest.Version; } set { _manifest.Version = value; _isDirty = true; } }
        public string Author { get { return _manifest.Author; } set { _manifest.Author = value; _isDirty = true; } }
        public DateTimeOffset DatePublished { get { return _manifest.DatePublished; } private set { _manifest.DatePublished = value; _isDirty = true; } }

        internal SmartInkPackage(string name,  IPackageStorageProvider provider)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty.");
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest =  new SmartInkManifest() { Name = name };
            _cts = new CancellationTokenSource();
        }

        internal SmartInkPackage(SmartInkManifest manifest,IPackageStorageProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException($"{nameof(provider)} cannot be null");

            _manifest = manifest ?? throw new ArgumentNullException($"{nameof(manifest)} cannot be null");
            _cts = new CancellationTokenSource();
        }

     

        //public Task RenameAsync(string newName)
        //{
        //    if (string.IsNullOrEmpty(newName))
        //        throw new ArgumentNullException($"{nameof(newName)} cannot be null or empty.");

        //    _manifest.Name = Name;
        //    _isDirty = true;
        //    return _provider.RenameAsync(newName);
        //}

        public IList<string> GetTags()
        {
            List<string> tags = new List<string>();
            foreach (var t in _manifest.IconMap.Keys)
            {
                tags.Add(t);
            }

            return tags;
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            if (_manifest.IconMap.ContainsKey(tag))
                return;

            _manifest.IconMap.Add(tag, null);
            _isDirty = true;
        }

        public async Task RemoveTagAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            if (!_manifest.IconMap.ContainsKey(tag))
                return;

            var icon = _manifest.IconMap[tag];
            _manifest.IconMap.Remove(tag);
            if (icon != null)
                await _provider.DeleteIconAsync(icon);
            _isDirty = true;
        }

        public async Task SaveIconAsync(string tag, IStorageFile file)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");

            if (file == null)
                throw new ArgumentNullException($"{nameof(file)} canot be null.");

            if (_manifest.IconMap.ContainsKey(tag))
            {
                await _provider.DeleteIconAsync(tag);
                _manifest.IconMap[tag] = file.Name;
            }
            else
                _manifest.IconMap.Add(tag, file.Name);

            await _provider.SaveIconAsync( file);
            _isDirty = true;
        }

        public Task SaveIconAsync(Guid tagId, IStorageFile file)
        {
            if (tagId == Guid.Empty)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty.");
            return SaveIconAsync(tagId.ToString(), file);
        }

        public async Task<IStorageFile> GetIconAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");
            if (!_manifest.IconMap.ContainsKey(tag) || string.IsNullOrWhiteSpace(_manifest.IconMap[tag]))
                return null;
            
            return await _provider.GetIconAsync(_manifest.IconMap[tag]);
        }

        public async Task<IStorageFile> GetIconAsync(Guid tagId)
        {
         
            if (tagId == null || Guid.Empty == tagId)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty");

            var icon = await GetIconAsync(tagId.ToString());
            return icon;
        }

        public async Task DownloadModelAsync(Uri modelUri)
        {
            if (modelUri == null)
                throw new ArgumentNullException($"{nameof(modelUri)} canot be null");

            //if (!modelUri.IsFile)
            //    throw new ArgumentOutOfRangeException($"{nameof(modelUri)} must be a file.");

            var filename =  System.IO.Path.GetFileName(modelUri.LocalPath);
            
            var tempFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            BackgroundDownloader downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(modelUri, tempFile);

            await HandleDownloadAsync(download, true);
            ModelDownloadCompleted?.Invoke(this, null);

            await _provider.SaveModelAsync(tempFile);
            
        }

        public async Task SaveAsync()
        {
            if (!_isDirty)
                return;
            await _provider.SaveManifestAsync(_manifest);
            _isDirty = false;
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            
            try
            {
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    ModelDownloadStarted?.Invoke(this, null);
                    await download.StartAsync().AsTask(_cts.Token,progressCallback);
                }
                else {
                    await download.AttachAsync().AsTask(_cts.Token, progressCallback);
                }

                var response = download.GetResponseInformation();

            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex) {
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
