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
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartInkLaboratory.ViewModels
{
    public class IconMapViewModel : ViewModelBase, IVisualState
    {
        //private IIconMappingService _iconMap;
        private IAppStateService _state;

        //private StorageFolder _storageFolder;

        public Tag CurrentTag
        {
            get { return _state.CurrentTag; }
            set { if (_state.CurrentTag == value)
                    return;
                _state.CurrentTag = value;
           
            }
        }

      

        private ImageSource _iconBitmap;

        public event EventHandler<VisualStateEventArgs> VisualStateChanged;

        public ImageSource Icon
        {
            get {
                
                return _iconBitmap; }
            set
            {
                if (_iconBitmap == value)
                    return;
                _iconBitmap = value;
                VisualStateChanged?.Invoke(this, new VisualStateEventArgs { NewState = "HasIcon" });
                RaisePropertyChanged(nameof(Icon));
            }
        }


        public RelayCommand OpenFile{ get; private set; }

        public string CurrentVisualState { get; private set; }

        public IconMapViewModel(IAppStateService state)
        {
            _state = state;

            _state.PackageChanged += async(s, e) => {
                if (_state.CurrentPackage == null)
                {
                    //_storageFolder = null;
                    VisualStateChanged.Invoke(this, new VisualStateEventArgs { NewState = "NoPackage" });
                    return;
                }
                //_storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_state.CurrentPackage.Name, CreationCollisionOption.OpenIfExists);
                VisualStateChanged.Invoke(this, new VisualStateEventArgs { NewState = "HasPackage" });
            };

            _state.TagChanged += async (s,e)=>{
                if (_state.CurrentPackage == null)
                    return;

                if (_state.CurrentTag != null)
                    await SetIconLocation(_state.CurrentTag.Id);
                OpenFile.RaiseCanExecuteChanged();
            };

            this.OpenFile = new RelayCommand(async() => {
                var picker = new FileOpenPicker();
                picker.ViewMode =PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png"); 
               

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await UpdateIconAsync(file);
                    await _state.CurrentPackage.SaveAsync();
                    _state.IconUpdated();
                }
            },
            ()=> {
                return _state.CurrentProject != null && CurrentTag != null; });

            
        }

        public async Task<IStorageFile> GetIconFileAsync(Guid currentTagId)
        {
        
            var icon = await _state.CurrentPackage.GetIconAsync(currentTagId);
            if (icon == null)
                icon = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Images/no_icon.png"));

            return icon;
        }

        private async Task UpdateIconAsync(IStorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException($"{nameof(file)} cannot be null.");

            if (CurrentTag == null)
                return;


            

            await LoadIconAsync(file);
            _state.IconUpdated();
            //return await CopyFileToLocalPackageFolder(file);
            await _state.CurrentPackage.SaveIconAsync(CurrentTag.Id, file);
        }

        private async Task LoadIconAsync(IStorageFile file)
        {
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                Icon = bitmapImage;

            }
        }

        private async Task DeleteOldIconAsync(string oldfile)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_state.CurrentProject.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var file = await folder.GetFileAsync(oldfile);
            await file.DeleteAsync();
        }

     

        private async Task SetIconLocation(Guid currentTagId)
        {
                var file = await GetIconFileAsync(currentTagId);
                await LoadIconAsync(file);
        }

      

    }
}
