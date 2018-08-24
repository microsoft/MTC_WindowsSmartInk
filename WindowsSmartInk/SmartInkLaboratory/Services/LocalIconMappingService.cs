//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Windows.Storage;

//namespace SmartInkLaboratory.Services
//{
//    public class LocalIconMappingService : IIconMappingService
//    {
//        Dictionary<Guid, string> _iconMap;
//        private ITagService _tagService;
//        private string _packageId;

//        public bool IsOpen { get; private set; }

//        public LocalIconMappingService(ITagService tagService)
//        {
//            _tagService = tagService;
//        }

//        public async Task OpenAsync(string packageId)
//        {
//            _packageId = packageId;
//            _iconMap = await GetIconMapAsync(_packageId);
//            IsOpen = true;
//        }

//        public bool Contains(Guid tag)
//        {
//            return _iconMap.ContainsKey(tag);
//        }
    
//        public async Task<string> GetTagIconAsync(Guid tag)
//        {
//            if (tag == null)
//                throw new ArgumentNullException($"{nameof(tag)} cannot be null or empty.");

//            if (_iconMap == null)
//                return null;

//            if (!_iconMap.ContainsKey(tag))
//                return null;

//            return _iconMap[tag];

//        }

        
//        public async Task AddTagIconAsync(Guid tag, string iconLocation)
//        {
//            if (tag == null || string.IsNullOrWhiteSpace(iconLocation))
//                throw new ArgumentNullException($"{nameof(tag)} and/or {nameof(iconLocation)} cannot be null or empty.");

//            if (_iconMap == null)
//                return;

//            if (_iconMap.ContainsKey(tag))
//                _iconMap[tag] = iconLocation;
//            else
//                _iconMap.Add(tag, iconLocation);
//            await SaveIconMapAsync(_packageId);
//        }

//        public async Task DeleteTagIconAsync(Guid tag)
//        {
//            if (tag == null )
//                throw new ArgumentNullException($"{nameof(tag)} cannot be null.");

//            if (_iconMap == null)
//                return; 

//            _iconMap.Remove(tag);

//            await SaveIconMapAsync(_packageId);
//        }

//        public async Task ClearIconMapAsync()
//        {
//            if (!IsOpen)
//                return;

//            if (_iconMap != null)
//                _iconMap.Clear();

//            var file = await GetConfigFileAsync(_packageId);
//            await file.DeleteAsync();
//        }

//        private async Task SaveIconMapAsync(string packageId)
//        {
//            var file = await GetConfigFileAsync(packageId);
            
//            var json = JsonConvert.SerializeObject(_iconMap);
//            await FileIO.WriteTextAsync(file, json);
//        }

        

//        private async Task<Dictionary<Guid,string>> GetIconMapAsync(string projectId)
//        {
//            Dictionary<Guid, string> iconMap;
//            var file = await GetConfigFileAsync(projectId);
//            var json = await FileIO.ReadTextAsync(file);
//            if (string.IsNullOrWhiteSpace(json))
//                iconMap = new Dictionary<Guid, string>();
//            else
//            {
//                iconMap = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(json);
//            }

//            return iconMap;
//        }

//        private async Task<StorageFile> GetConfigFileAsync(string projectId)
//        {
//            var folder = ApplicationData.Current.LocalFolder;
//            var file = await folder.CreateFileAsync($"{projectId}.cfg", CreationCollisionOption.OpenIfExists);
//            return file;
//        }
//    }
//}
