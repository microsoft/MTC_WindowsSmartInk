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
using AMP.ViewModels;
using GalaSoft.MvvmLight.Command;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Micosoft.MTC.SmartInk.Package;
using Windows.UI.Input.Inking;
using Microsoft.MTC.SmartInk.Extensions;

namespace SmartInkLaboratory.ViewModels
{
    public class InkPredictionEventArgs : EventArgs
    {
        public string tag { get; set; }
    }





    public class TestViewModel : AsyncViewModel, IInkProcessor, IVisualState
    {
        private ITrainingService _training;
        private IPredictionService _prediction;
        private IAppStateService _state;


        public Tag CurrentTag
        {
            get { return _state.CurrentTag; }
            set
            {

                _state.CurrentTag = value;
                RaisePropertyChanged(nameof(CurrentTag));
            }
        }

        private Iteration _selectedIteration;
        public Iteration SelectedIteration
        {
            get { return _selectedIteration; }
            set
            {
                if (_selectedIteration == value)
                    return;
                _selectedIteration = value;
                RaisePropertyChanged(nameof(SelectedIteration));
            }
        }


        private string _tagResult;
        public string TagResult
        {
            get { return _tagResult; }
            set
            {
                if (_tagResult == value)
                    return;
                _tagResult = value;
                RaisePropertyChanged(nameof(TagResult));
            }
        }

        private string _evaluationResult;

        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public string EvaluationResult
        {
            get { return _evaluationResult; }
            set
            {
                if (_evaluationResult == value)
                    return;
                _evaluationResult = value;
                RaisePropertyChanged(nameof(EvaluationResult));
            }
        }

        private double _downloadProgress;
        public double DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                if (_downloadProgress == value)
                    return;
                _downloadProgress = value;
                RaisePropertyChanged(nameof(DownloadProgress));
            }
        }

        private bool _useRemoteService = true;
        public bool UseRemoteService
        {
            get { return _useRemoteService; }
            set
            {
                if (_useRemoteService == value)
                    return;
                _useRemoteService = value;
                RaisePropertyChanged(nameof(UseRemoteService));
            }
        }

        private bool _isLocalModelAvailable;
        public bool IsLocalModelAvailable
        {
            get { return _isLocalModelAvailable; }
            set
            {
                if (_isLocalModelAvailable == value)
                    return;
                _isLocalModelAvailable = value;
                RaisePropertyChanged(nameof(IsLocalModelAvailable));
            }
        }

        private bool _isReadyToTest;
        public bool IsReadyToTest
        {
            get { return IsLocalModelAvailable || Iterations.Count > 0; }
           
        }



        public ObservableCollection<Iteration> Iterations { get; set; } = new ObservableCollection<Iteration>();

        public RelayCommand UploadCorrection { get; set; }
        public RelayCommand DownloadModel { get; private set; }

        public string CurrentVisualState => throw new NotImplementedException();

        public TestViewModel(ITrainingService training, IPredictionService prediction, IAppStateService state)
        {
            _training = training;
            _prediction = prediction;
            _state = state;
            _state.KeysChanged += (s, e) =>
            {
                prediction.Initialize(_state.CurrentKeys.PredicationKey);
            };

            _state.PackageChanged += async  (s, e) => {
                var iterations = await _training.GetIterationsAysnc();
                foreach (var i in iterations)
                    Iterations.Add(i);

                
                SelectedIteration = (Iterations.Count > 0) ?Iterations[0] : null;
                IsLocalModelAvailable = (_state.CurrentPackage == null) ? false : _state.CurrentPackage.IsLocalModelAvailable;

                RaisePropertyChanged(nameof(IsReadyToTest));

                if (_state.CurrentPackage == null || !IsReadyToTest)
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "NoPackage" });
                else
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "HasPackage" });
            };

           
            this.UploadCorrection = new RelayCommand(() =>
            {
            },
            () =>
            {
                if (EvaluationResult == null || _state.CurrentTag == null)
                    return false;

                return _state.CurrentTag.Name != EvaluationResult;
            });

            this.DownloadModel = new RelayCommand(async() => {
                var downloadUri = await _training.GetModuleDownloadUriAsync(SelectedIteration.Id);
                if (downloadUri != null)
                {
                    var manager = new PackageManager();
                    manager.ModelDownloadStarted += (s, e) => {
                        VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "DownloadStarted" });
                    };
                    manager.ModelDownloadCompleted += (s, e) => {
                        VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "DownloadCompleted" });
                    };
                    manager.ModelDownloadError += (s, e) => {
                        VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "DownloadError" });
                    };
                    manager.ModelDownloadProgress += (s, e) => {
                        DownloadProgress = e.Percentage;
                    };
                    var model = await manager.DownloadModelAsync(downloadUri);
                    await _state.CurrentPackage.SaveModelAsync(model);
                    IsLocalModelAvailable = _state.CurrentPackage.IsLocalModelAvailable;
                }
            });

        }

      

        public async Task<IDictionary<string, float>> ProcessInkAsync(IList<InkStroke> strokes)
        {
            IDictionary<string, float> result = null;
            if (IsLocalModelAvailable)
                result = await _state.CurrentPackage.EvaluateAsync(strokes);
            else
            {
                result = await GetPredictionFromServiceAsync(strokes);
            }
            ProcessModelOutput(result);
            return result;
        }

        private async Task<IDictionary<string, float>> GetPredictionFromServiceAsync(IList<InkStroke> strokes)
        {
            var bitmap = strokes.DrawInk();
            
            using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(bitmap);
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                try
                {
                    await encoder.FlushAsync();
                    var result = await _prediction.GetPredictionAsync(stream.AsStreamForRead(), _state.CurrentProject.Id);
                    return result;
                }
                catch (Exception err)
                {
                    return null;
                }
            }
        }

        private void ProcessModelOutput(IDictionary<string,float> output)
        {
            if (EvaluationResult == null)
            {
                TagResult = "null";
                EvaluationResult = "Error";
                return;
            }

            TagResult = output.Keys.ToList()[0];
            var evalResult = string.Empty;
            foreach (var item in output)
            {
                evalResult += $"{item.Key} [{item.Value}]{Environment.NewLine}";
            }

            EvaluationResult = evalResult;

            UploadCorrection.RaiseCanExecuteChanged();
        }

        protected override async Task InitializeAsync()
        {
        
        }
    }
}
