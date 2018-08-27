using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Micosoft.MTC.SmartInk.Package.Storage
{
    internal class LocalAppDataPackageManagerStorageProvider : IPackageManagerStorageProvider
    {
        private readonly string ROOT_PATH;
        private IStorageFolder _root;


        public LocalAppDataPackageManagerStorageProvider()
        {
            ROOT_PATH = ApplicationData.Current.LocalFolder.Path + "\\SmartInkPackages";
            if (!Directory.Exists(ROOT_PATH))
                Directory.CreateDirectory(ROOT_PATH);
        }

        public async Task DeletePackageAsync(string packagename)
        {
            if (string.IsNullOrWhiteSpace(packagename))
                return;

            if (_root == null)
                _root = await GetRootFolderAsync();
            try
            {
                var packageFolder = await _root.GetFolderAsync(packagename);
                await packageFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<IPackageStorageProvider> CreatePackageProviderAsync(string packagename, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(packagename))
                throw new ArgumentNullException($"{nameof(packagename)} cannot be null or empty.");

            if (_root == null)
                _root = await GetRootFolderAsync();

            if (Directory.Exists($"{_root.Path}\\{packagename}") && !overwrite)
                throw new InvalidOperationException($"{packagename} already exists.");

            return await CreatePackageStorageProviderAsync(packagename);
        }

        public async Task<SmartInkPackage> GetPackageAsync(string packagename)
        {
            if (string.IsNullOrWhiteSpace(packagename))
                throw new ArgumentNullException($"{nameof(packagename)} cannot be null or empty.");

            if (_root == null)
                _root = await GetRootFolderAsync();

            var packageFolder = await _root.GetFolderAsync(packagename);
            if (packageFolder == null)
                return null;

            if (!File.Exists($"{packageFolder.Path}\\manifest.json"))
                return null;

            var package = await packageFolder.GetFileAsync("manifest.json");
            if (package == null)
                return null;

            var json = await FileIO.ReadTextAsync(package);
            if (string.IsNullOrWhiteSpace(json))
                return new SmartInkPackage(packagename, await GetPackageStorageProviderAsync(packagename));
            var manifest = JsonConvert.DeserializeObject<SmartInkManifest>(json);

            return new SmartInkPackage(manifest, await GetPackageStorageProviderAsync(packagename));

        }

        public async Task<IList<string>> GetInstalledPackagesAsync()
        {
            if (_root == null)
                _root = await GetRootFolderAsync();

            var packages = new List<string>();
            var folders = await _root.GetFoldersAsync();
            foreach (var f in folders)
            {
                packages.Add(f.Name);
            }

            return packages;
        }

        private async Task<IPackageStorageProvider> CreatePackageStorageProviderAsync(string packagename)
        {
            if (string.IsNullOrWhiteSpace(packagename))
                throw new ArgumentNullException($"{nameof(packagename)} cannot be null or empty.");

            if (_root == null)
                _root = await GetRootFolderAsync();

            var folder = await _root.CreateFolderAsync(packagename, CreationCollisionOption.FailIfExists);
            return new LocalAppDataPackageStorageProvider(folder);
        }

        private async Task<IPackageStorageProvider> GetPackageStorageProviderAsync(string packagename)
        {
            if (string.IsNullOrWhiteSpace(packagename))
                throw new ArgumentNullException($"{nameof(packagename)} cannot be null or empty.");

            if (_root == null)
                _root = await GetRootFolderAsync();
           
            var folder = await _root.GetFolderAsync(packagename);
       
            return new LocalAppDataPackageStorageProvider(folder);
        }

        private async Task<IStorageFolder> GetRootFolderAsync()
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(ROOT_PATH);
            return folder;
        }
    }

    internal class LocalAppDataPackageStorageProvider : IPackageStorageProvider
    {
        readonly IStorageFolder _root;
        public LocalAppDataPackageStorageProvider(IStorageFolder root)
        {
            _root = root ?? throw new ArgumentNullException($"{nameof(root)} cannot be null or empty.");
        }

        //public async Task RenameAsync(string newName)
        //{
        //    if (string.IsNullOrEmpty(newName))
        //        throw new ArgumentNullException($"{nameof(newName)} cannot be null or empty.");

        //    await _root.RenameAsync(newName, NameCollisionOption.FailIfExists);
        //}

             
        public async Task SaveIconAsync(IStorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException($"{nameof(file)} cannot be null.");

            var folder = await _root.CreateFolderAsync("Icons", CreationCollisionOption.OpenIfExists);

            await file.CopyAsync(folder, file.Name, NameCollisionOption.ReplaceExisting);
        }

        public async Task<IStorageFile> GetIconAsync(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentNullException($"{nameof(icon)} cannot be null or empty.");

            var folder = await _root.CreateFolderAsync("Icons",CreationCollisionOption.OpenIfExists);
            var file = await folder.GetFileAsync(icon);
            return file;
        }

        public async Task DeleteIconAsync(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentNullException($"{nameof(icon)} cannot be null or empty.");

            var folder = await _root.CreateFolderAsync("Icons", CreationCollisionOption.OpenIfExists);
            var file = await folder.GetFileAsync(icon);
            await file.DeleteAsync();
        }

        public async Task<IStorageFile> GetModelAsync(string filename)
        {
            try
            {
                var folder = await _root.GetFolderAsync("Model");
                var file = await folder.GetFileAsync(filename);
                return file;
            }
            catch (Exception)
            {

                return null;
            }
        }
           

        public async Task SaveModelAsync(IStorageFile model)
        {
            if (model == null)
                throw new ArgumentNullException($"{nameof(model)} cannot be null.");

            var folder = await _root.CreateFolderAsync("Model", CreationCollisionOption.OpenIfExists);
            await model.CopyAsync(folder, model.Name, NameCollisionOption.ReplaceExisting);
        }

        public async Task SaveManifestAsync(SmartInkManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException($"{nameof(manifest)} cannot be null");

            var json = JsonConvert.SerializeObject(manifest);
            var file = await _root.CreateFileAsync("manifest.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public async Task<SmartInkManifest> GetManifestAsync()
        {
            try
            {
                var file = await _root.GetFileAsync("manifest.json");
                if (file != null){
                    var json = await FileIO.ReadTextAsync(file);
                    var manifest = JsonConvert.DeserializeObject<SmartInkManifest>(json);
                    return manifest;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
        
    }

}