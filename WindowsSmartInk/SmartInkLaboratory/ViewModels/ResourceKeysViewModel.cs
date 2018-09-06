/*
 * Copyright(c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using SmartInkLaboratory.Views.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value)
                    return;
      
                _isOpen = value;
                Debug.WriteLine($"IsOpen Property Changed {_isOpen}");
                RaisePropertyChanged(nameof(IsOpen));
            }
        }


        public RelayCommand ShowKeys { get; set; }
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

            this.ShowKeys = new RelayCommand(() => { IsOpen = true; });

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
                IsOpen = false;
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
            if ( _keys.Keys.Count == 0)
                return;

            if (last == null || !_keys.ContainsKey(last))
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

       
    }
}
