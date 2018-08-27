using SmartInkLaboratory.AI;
using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

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

            _state.PackageChanged +=async  (s, e) => {
                var iterations = await _training.GetIterationsAysnc();
                foreach (var i in iterations)
                {
                    Iterations.Add(i);
                }
                SelectedIteration = Iterations[0];
            };

            _state.PackageChanged += (s, e) =>
            {
                if (_state.CurrentPackage == null)
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
                    await _state.CurrentPackage.DownloadModelAsync(downloadUri);
                }
            });

            //prediction.Initialize(_state.CurrentKeys.PredicationKey);
        }

        public async Task<IDictionary<string, float>> ProcessInkImageAsync(WriteableBitmap bitmap)
        {
            try
            {
                using (var memStream = new InMemoryRandomAccessStream())
                {

                    await bitmap.ToStream(memStream, BitmapEncoder.PngEncoderId);
                    var result = await _prediction.GetPrediction(memStream.AsStreamForRead(), _state.CurrentProject.Id, SelectedIteration.Id);
                    ProcessModelOutput(result);
                    return result;
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        private void ProcessModelOutput(IDictionary<string , float > output)
        {
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
