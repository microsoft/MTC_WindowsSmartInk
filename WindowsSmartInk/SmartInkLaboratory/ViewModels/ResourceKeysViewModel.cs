using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Views.Dialogs;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace SmartInkLaboratory.ViewModels
{
    public class ResourceKeysViewModel : ViewModelBase
    {
        private IClassifierService _classifier;
        private ISecureKeyService _keyService;
        private SmartInkLaboratory.Services.UX.IDialogService _dialog;
        private IAppStateService _state;
        private INavigationService _nav;
        Dictionary<string, ResourceKeys> _keys = new Dictionary<string, ResourceKeys>();
        public ObservableCollection<string> KeyList { get; private set; } = new ObservableCollection<string>();

        public event EventHandler ResourceKeyChanged;

        private bool _isDirty;

        private string _resource;
        public string Resource
        {
            get { return _resource; }
            set
            {
                if (_resource == value)
                    return;
                _resource = value;
                _isDirty = true;
                RaisePropertyChanged(nameof(Resource));
                SaveKeys.CanExecute(null);
            }
        }

        private string _trainingKey;
        public string TrainingKey
        {
            get { return _trainingKey; }
            set
            {
                if (_trainingKey == value)
                    return;
                _trainingKey = value;
                _isDirty = true;
                RaisePropertyChanged(nameof(TrainingKey));
                SaveKeys.CanExecute(null);
            }
        }

        private string _predictionKey;
        public string PredictionKey
        {
            get { return _predictionKey; }
            set
            {
                if (_predictionKey == value)
                    return;
                _predictionKey = value;
                _isDirty = true;
                RaisePropertyChanged(nameof(PredictionKey));
                SaveKeys.CanExecute(null);
            }
        }


        private ResourceKeys _currentKeys;
        public ResourceKeys CurrentKeys
        {
            get { return _currentKeys; }
            set
            {
                _currentKeys = value;
                RaisePropertyChanged(nameof(CurrentKeys));
            }
        }


        public RelayCommand<string> SelectKey { get; private set; }
        public RelayCommand SaveKeys { get; private set; }
        public RelayCommand More { get; private set; }
        public RelayCommand<string> DeleteKey { get; private set; }
   



        public ResourceKeysViewModel(IClassifierService classifier, ISecureKeyService keys, SmartInkLaboratory.Services.UX.IDialogService dialog, IAppStateService state, INavigationService nav)
        {

            _classifier = classifier;
            _keyService = keys;
            _dialog = dialog;
            _state = state;
            _nav = nav;

            this.SelectKey = new RelayCommand<string>((resource) => {

                if (string.IsNullOrWhiteSpace(resource))
                    return;

                if (!_keys.ContainsKey(resource))
                    throw new InvalidOperationException("Resource not found");

                var key = SetKeys(resource);
                _state.CurrentKeys = key;
            });

            this.SaveKeys = new RelayCommand(() => {
                _keyService.SaveKeys(Resource, TrainingKey, PredictionKey);
                ApplicationData.Current.LocalSettings.Values["LastResource"] = Resource;
                _state.CurrentKeys = (new ResourceKeys { Resource = Resource, TrainingKey = TrainingKey, PredicationKey = PredictionKey });
            },
            () => {
                return !string.IsNullOrWhiteSpace(Resource) &&
                       !string.IsNullOrWhiteSpace(TrainingKey) &&
                       !string.IsNullOrWhiteSpace(PredictionKey) &&
                       _isDirty;
            });

            this.More = new RelayCommand(async() => {
                await _dialog.OpenAsync<ResourceKeysViewModel>(DialogKeys.ResourceList, this);
            });

            this.DeleteKey = new RelayCommand<string>((key) => {
                _keyService.DeleteKey(key);
                KeyList.Remove(key);
            });

            Load();
        }

        public void Load()
        {
            KeyList.Clear();
            _keys.Clear();
            var savedKeys = _keyService.GetKeys();
            
            foreach (var k in savedKeys)
            {
                KeyList.Add(k.Resource);
                _keys.Add(k.Resource, k);
            }

            
            var last = ApplicationData.Current.LocalSettings.Values["LastResource"] as string;
            if (last == null || _keys.Keys.Count == 0)
                return;

            if (!_keys.ContainsKey(last))
                last = savedKeys[0].Resource;

            var keys = SetKeys(last);
            _state.CurrentKeys = (keys);
            
        }

        private ResourceKeys SetKeys(string resource)
        {
            var key = _keys[resource];
            Resource = key.Resource;
            TrainingKey = key.TrainingKey;
            PredictionKey = key.PredicationKey;
            ApplicationData.Current.LocalSettings.Values["LastResource"] = resource;
            CurrentKeys = key;
            _classifier.SetKeys(key);
            ResourceKeyChanged?.Invoke(this, null);
            return key;
        }

        //protected override async Task InitializeAsync()
        //{
        //    Load();

        //}
    }
}
