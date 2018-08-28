using Micosoft.MTC.SmartInk.Package.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
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
        CancellationTokenSource _cts;

        private SmartInkManifest _manifest;
        private IPackageStorageProvider _provider;

        public event EventHandler ModelDownloadStarted;
        public event EventHandler ModelDownloadCompleted;
        public event EventHandler ModelDownloadError;
        public event EventHandler<DownloadProgressEventArgs> ModelDownloadProgress;

        public string Name { get { return _manifest.Name; } }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public DateTimeOffset DatePublished { get; set; }

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
            foreach (var t in _manifest.TagList.Values)
                tags.Add(t);

            return tags;
        }

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

        public Task<IStorageFile> GetIconAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");

            Guid tagId;
            if (Guid.TryParse(tag, out tagId))
                return GetIconAsync(tagId);

            throw new ArgumentException($"{nameof(tag)}:{tag} is not a valid guid");
        }

        public async Task<IStorageFile> GetIconAsync(Guid tagId)
        {
            if (tagId == null || Guid.Empty == tagId)
                throw new ArgumentNullException($"{nameof(tagId)} cannot be null or empty");

            if (!_manifest.IconMap.ContainsKey(tagId))
                return null;

            return await _provider.GetIconAsync(_manifest.IconMap[tagId]);
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
            await _provider.SaveManifestAsync(_manifest);
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
