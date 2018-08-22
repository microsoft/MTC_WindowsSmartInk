using Micosoft.MTC.SmartInk.Package.Storage;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package
{
    public class PackageManager
    {
        private IPackageManagerStorageProvider _provider;

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
    }
}
