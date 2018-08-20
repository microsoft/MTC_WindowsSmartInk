using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartInkLaboratory.Services
{
    public class LocalProjectPackageMapper : IProjectPackageMapper
    {
        private readonly string PACKAGE_FILE_NAME = "package_map.json";
        static Dictionary<string, string> _map;

        public LocalProjectPackageMapper()
        {
            
        }

        public async Task<IList<string>> GetAllPackagesAsync()
        {
            if (_map == null)
                await OpenMapAsync();

            return _map.Keys.ToList();
        }

        public async Task<IList<string>> GetPackagesByProjectAsync(string project)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentNullException($"{nameof(project)} cannot be null or empty.");

            if (_map == null)
                await OpenMapAsync();

            var result = new List<String>();


            var matches = _map.Where(kvp => kvp.Value == project).Select(kvp => kvp.Key);
           
            return matches.ToList();
        }

        public async Task AddAsync(string package, string project)
        {
            if (string.IsNullOrWhiteSpace(package))
                throw new ArgumentNullException($"{nameof(package)} cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentNullException($"{nameof(project)} cannot be null or empty.");


            if (_map == null)
                await OpenMapAsync();

            if (_map.ContainsKey(package.ToLower()))
                _map[package] = project;
            else
                _map.Add(package.ToLower(), project);

            await SaveMapAsync();
       
        }

        public async Task DeleteAsync(string package)
        {
            if (string.IsNullOrWhiteSpace(package))
                throw new ArgumentNullException($"{nameof(package)} cannot be null or empty.");
                    
            if (_map == null)
                await OpenMapAsync();

            if (!_map.ContainsKey(package))
                return;

            _map.Remove(package);

            await SaveMapAsync();
        }

        private async Task OpenMapAsync()
        {
            var file = await GetFileAsync();
            var json = await FileIO.ReadTextAsync(file);
            if (!string.IsNullOrWhiteSpace(json))
                _map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            else
                _map = new Dictionary<string, string>();
        }

        private async Task SaveMapAsync()
        {
            if (_map == null)
                return;

            var json = JsonConvert.SerializeObject(_map);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var file = await GetFileAsync();
                await FileIO.WriteTextAsync(file, json);
            }
        }

        private async Task<IStorageFile> GetFileAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(PACKAGE_FILE_NAME, CreationCollisionOption.OpenIfExists);
            return file;
        }
    }
}
