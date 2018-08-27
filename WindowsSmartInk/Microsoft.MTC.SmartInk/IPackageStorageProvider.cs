using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package.Storage
{
    public interface IPackageStorageProvider
    {
        //Task RenameAsync(string newName);
        Task SaveIconAsync(IStorageFile file);
        Task<IStorageFile> GetIconAsync(string tag);
        Task DeleteIconAsync(string icon);
        Task SaveModelAsync(IStorageFile model);
        Task<IStorageFile> GetModelAsync(string filename);
        Task SaveManifestAsync(SmartInkManifest manifest);
        Task<SmartInkManifest> GetManifestAsync();
    }

    public interface IPackageManagerStorageProvider
    {
        Task<SmartInkPackage> GetPackageAsync(string packagename);
        Task<IPackageStorageProvider> CreatePackageProviderAsync(string packagename, bool overwrite = false);
        Task  DeletePackageAsync(string packagename);
        Task<IList<string>> GetInstalledPackagesAsync();
    }
}