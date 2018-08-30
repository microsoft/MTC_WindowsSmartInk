using Micosoft.MTC.SmartInk.Package.Storage;
using Microsoft.MTC.SmartInk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Micosoft.MTC.SmartInk.Package
{
   

    public class SmartInkPackage
    {
  

        private SmartInkManifest _manifest;
        private IPackageStorageProvider _provider;
        private Model _model;

   

        public string Name { get { return _manifest.Name; } }
        public string Description { get; set; } = "Windows 10 SmartInk Package";
        public string Version { get; set; } = "1.0.0.0";
        public string Author { get; set; }
        public DateTimeOffset DatePublished { get; set; }

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

      

        public async Task<IDictionary<string, float>> EvaluateAsync(SoftwareBitmap bitmap)
        {
            if (string.IsNullOrWhiteSpace(_manifest.Model))
                throw new InvalidOperationException("Model file not available");

            var modelfile = await _provider.GetModelAsync(_manifest.Model);

            if (modelfile == null)
                throw new InvalidOperationException("Model file not found");

            _model = await Model.CreateModelAsync(modelfile, _manifest.TagList.Values.ToList());
            var result = await _model.EvaluateAsync(bitmap);
            return result.loss;
        }

        public async Task SaveAsync()
        {
            await _provider.SaveManifestAsync(_manifest);
        }

        public async Task SaveModelAsync(IStorageFile modelFile)
        {
            await _provider.SaveModelAsync(modelFile);
            _manifest.Model = modelFile.Name;
            await SaveAsync();
        }

        
    }
}
