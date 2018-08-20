using SmartInkLaboratory.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartInkLaboratory.Services
{
    public class InkManifestManager : IInkManifestManager
    {
        InkManifest _manifest = new InkManifest();
        public InkManifest Current { get { return _manifest; } private set { _manifest = value; }  }

        public async Task SaveAsync()
        {
            var json = JsonConvert.SerializeObject(_manifest);
            var file = await GetConfigFileAsync(_manifest.ProjectId.ToString());
            await FileIO.WriteTextAsync(file,json);
        }

        public async Task LoadAsync(string projectId)
        {
            var file = await GetConfigFileAsync(projectId);
            var json = await FileIO.ReadTextAsync(file);
            Current = JsonConvert.DeserializeObject<InkManifest>(json);
        }

        private async Task<StorageFile> GetConfigFileAsync(string projectId)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync($"{projectId}.cfg", CreationCollisionOption.OpenIfExists);
            return file;
        }
    }
}
