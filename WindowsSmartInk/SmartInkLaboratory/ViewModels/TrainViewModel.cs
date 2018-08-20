using SmartInkLaboratory.Services;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartInkLaboratory.ViewModels
{
    public class TrainViewModel : ViewModelBase, IVisualState, IInkProcessor
    {
        public string CurrentVisualState => throw new NotImplementedException();

        private CoreDispatcher _dispatcher;

        
        IStorageFolder _storageFolder;
  
        private IImageService _images;
        private IIconMappingService _mapper;
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

        

        public TrainViewModel(IImageService images, ITrainingService train, IIconMappingService mapper, IAppStateService state)
        {
            _images = images;
            _mapper = mapper;
            _train = train;
            _state = state;
            
            _state.TagChanged += async (s,e) => { 
                var iconfile = await GetIconFileAsync(_state.CurrentTag.Id);
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
                    await _train.TrainAsync();
                },
                ()=> { return true; // _uploadComplete || TotalImageCount == 0; 
                });

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public async Task<(string tag,double probability)> ProcessInkImageAsync(WriteableBitmap bitmap)
        {
           
            InkDrawing = bitmap;
            await  SaveBitmapAsync(bitmap);
            return (null, 0);
        }
        private  async Task SaveBitmapAsync(WriteableBitmap cropped)
        {
            StorageFolder pictureFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("SmartInk", CreationCollisionOption.OpenIfExists);
            var projectFolder = await pictureFolder.CreateFolderAsync(_state.CurrentPackage.Name, CreationCollisionOption.OpenIfExists);
            var tagFolder = await projectFolder.CreateFolderAsync(_state.CurrentTag.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var savefile = await tagFolder.CreateFileAsync($"{Guid.NewGuid().ToString()}.jpg", CreationCollisionOption.ReplaceExisting);
            if (cropped.PixelHeight < 256 || cropped.PixelWidth < 256)
            {
                var height = cropped.PixelHeight < 256 ? 256 : cropped.PixelHeight; ;
                var width = cropped.PixelWidth < 256 ? 256 : cropped.PixelWidth;
                cropped = cropped.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            using (IRandomAccessStream stream = await savefile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await cropped.ToStreamAsJpeg(stream);
                await stream.FlushAsync();

            }

            TotalImageCount++; _uploadComplete = false; Train.RaiseCanExecuteChanged();
        }

        private async Task<IStorageFile> GetIconFileAsync(Guid currentTagId)
        {
            IStorageFile file;
            var icon = await _mapper.GetTagIconAsync(currentTagId);
            if (string.IsNullOrWhiteSpace(icon))
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Images/no_icon.png"));
            else
            {
                if (_storageFolder == null)
                    _storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_state.CurrentPackage.Name, CreationCollisionOption.OpenIfExists);
                file = await _storageFolder.GetFileAsync(icon);
                
            }
            return file;
        }

        private async Task LoadIconAsync(IStorageFile file)
        {
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
