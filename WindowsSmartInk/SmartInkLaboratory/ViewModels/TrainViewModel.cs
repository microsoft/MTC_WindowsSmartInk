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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.UI.Input.Inking;
using Microsoft.MTC.SmartInk.Extensions;

namespace SmartInkLaboratory.ViewModels
{
    public class TrainViewModel : ViewModelBase, IVisualState, IInkProcessor
    {
        public string CurrentVisualState => throw new NotImplementedException();

        private CoreDispatcher _dispatcher;

        
        IStorageFolder _storageFolder;
  
        private IImageService _images;
        private ITrainingService _train;
        private IAppStateService _state;
        //private Guid _currentProject;

        private int _totalImageCount;

        private int _imageUploadCount;

        private StorageFileQueryResult _query;
        private bool _uploadComplete = false;


        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public int TotalImageCount
        {
            get { return _totalImageCount; }
            set
            {
                if (_totalImageCount == value)
                    return;
                _totalImageCount = value;
                _uploadComplete = _totalImageCount == 0;
                Upload.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(TotalImageCount));
            }
        }



        public int ImageUploadCount
        {
            get { return _imageUploadCount; }
            set
            {
                if (_imageUploadCount == value)
                    return;
                _imageUploadCount = value;
               
                RaisePropertyChanged(nameof(ImageUploadCount));
            }
        }


        private ImageSource _iconBitmap;
        public ImageSource TargetIcon
        {
            get
            {

                return _iconBitmap;
            }
            set
            {
                if (_iconBitmap == value)
                    return;
                _iconBitmap = value;
                RaisePropertyChanged(nameof(TargetIcon));
            }
        }

        private ImageSource _inkDrawing;
        public ImageSource InkDrawing
        {
            get { return _inkDrawing; }
            set
            {
                if (_inkDrawing == value)
                    return;
                _inkDrawing = value;
                RaisePropertyChanged(nameof(InkDrawing));
            }
        }


        public RelayCommand Upload { get; set; }
        public RelayCommand Train { get; set; }

        

        public TrainViewModel(IImageService images, ITrainingService train,  IAppStateService state)
        {
            _images = images;
            _train = train;
            _state = state;
            
            _state.TagChanged += async (s,e) => {
                var iconfile = await GetIconFileAsync(_state.CurrentTag.Id);
                if (iconfile != null)
                    await LoadIconAsync(iconfile);
              
             };

            _state.IconChanged += async (s, e) => {
                var iconfile = await GetIconFileAsync(_state.CurrentTag.Id);
                if (iconfile != null)
                    await LoadIconAsync(iconfile);
            };

            _state.PackageChanged += async (s, e) => {
              
                if (_state.CurrentPackage == null)
                {
                    TotalImageCount = 0;
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "NoPackage" });
                    return;
                }
                var files = await CreateFileListAsync();
                TotalImageCount = files.Count;
                VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "HasPackage" });
            };

            this.Upload = new RelayCommand(
                async() => {
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "Uploading" });
                    await UploadImagesAsync();
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "Waiting" });
                },
                ()=> { return TotalImageCount > 0; });
            this.Train = new RelayCommand(
                async () => {
                    VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "Training" });
                    var iteration = await _train.TrainAsync();
                    if (iteration != null)
                        VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "TrainingFinished" });
                    else
                        VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "TrainingError" });
                },
                ()=> { return _uploadComplete || TotalImageCount == 0; 
                });

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

       
        public async Task<IDictionary<string, float>> ProcessInkAsync(IList<InkStroke> strokes)
        {
            var inkBitmap = strokes.DrawInk();
            await SetImageSourceAsync(inkBitmap);
            var saveFile = await GetBitmapSaveFile();
            SaveSoftwareBitmapToFile(inkBitmap, saveFile);
            TotalImageCount++;
            return null;
        }

        private async Task SetImageSourceAsync(SoftwareBitmap bitmap)
        {
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);
            InkDrawing = source;
        }


        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    throw;
                }
            }
        }
        

        private async Task<StorageFile> GetBitmapSaveFile()
        {
            StorageFolder pictureFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("SmartInk", CreationCollisionOption.OpenIfExists);
            var projectFolder = await pictureFolder.CreateFolderAsync(_state.CurrentPackage.Name, CreationCollisionOption.OpenIfExists);
            var tagFolder = await projectFolder.CreateFolderAsync(_state.CurrentTag.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var savefile = await tagFolder.CreateFileAsync($"{Guid.NewGuid().ToString()}.jpg", CreationCollisionOption.ReplaceExisting);
            return savefile;
        }

        private async Task<IStorageFile> GetIconFileAsync(Guid currentTagId)
        {
            if (_state.CurrentPackage == null)
                return null;

            var icon = await _state.CurrentPackage.GetIconAsync(currentTagId);
            if (icon == null)
                icon = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Images/no_icon.png"));
           
            return icon;
        }

        private async Task LoadIconAsync(IStorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException($"{nameof(file)} cannot be null");

            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                TargetIcon = bitmapImage;
            }
        }

        private async Task UploadImagesAsync()
        {
            
            var files = await CreateFileListAsync();
            foreach (var file in files)
            {
                var tagId = (await file.GetParentAsync()).Name;
                var tags = new List<string> { tagId };
                if (await _images.UploadImageAsync(file, tags))
                {
                    await file.DeleteAsync();
                    ImageUploadCount++;
                }
            }
            ImageUploadCount = TotalImageCount = 0;
            _uploadComplete = true;
            Train.RaiseCanExecuteChanged();
        }

        private async Task<List<StorageFile>> CreateFileListAsync()
        {
            StorageFolder pictureFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("SmartInk", CreationCollisionOption.OpenIfExists);
            var projectFolder = await pictureFolder.CreateFolderAsync(_state.CurrentPackage.Name, CreationCollisionOption.OpenIfExists);
            //await SetupWatcher(projectFolder);
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".jpeg");
            fileTypeFilter.Add(".png");
            var options = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter);
           
            
            var folders = await projectFolder.GetFoldersAsync();

            List<StorageFile> files = new List<StorageFile>();
            TotalImageCount = 0;
            foreach (var folder in folders)
            {
                var images = await folder.GetFilesAsync();
                TotalImageCount += images.Count;
                files.AddRange(images);
            }

            return files;
        }

        // private async Task SetupWatcher(StorageFolder targetFolder)
        //{
        //    List<string> fileTypeFilter = new List<string>();
        //    fileTypeFilter.Add(".jpg");
        //    fileTypeFilter.Add(".png");
        //    fileTypeFilter.Add(".jpeg");
        //    fileTypeFilter.Add(".gif");
        //    var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);


        //    _query = targetFolder.CreateFileQueryWithOptions(queryOptions);
        //    IReadOnlyList<StorageFile> fileList = await _query.GetFilesAsync();
        //    _query.ContentsChanged += async (s, e) =>
        //    {
        //        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                TotalImageCount++; _uploadComplete = false; Train.RaiseCanExecuteChanged();
        //            });
        //    };
        //    await _query.GetFilesAsync();
        //}


    }
}
