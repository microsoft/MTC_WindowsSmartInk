
using SmartInkLaboratory.ViewModels;
using AMP.Views;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using SmartInkLaboratory.Extensions;
using Microsoft.Graphics.Canvas;
using SmartInkLaboratory.AI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartInkLaboratory
{

    public enum InteractionMode
    {
        Mapping,
        Training,
        Testing,
        Packaging
    }

    public struct Icon
    {
        public string Id { get; private set; }
        public string Path { get; private  set; } 
        public Rect BoundingBox { get; private set; }

        public Icon(string path, Rect bounding)
        {
            Id = Guid.NewGuid().ToString();
            Path = path;
            BoundingBox = bounding;
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : NavAwarePage
    {
        private string _currentProject;
        private string _currentTag;

        DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(750) };
        List<InkStroke> _sessionStrokes = new List<InkStroke>();
        List<InkStroke> _allStrokes = new List<InkStroke>();
        private InteractionMode _interactionMode;
        private InkModel _inkModel;

     

        private InkSynchronizer _inkSync;
        IReadOnlyList<InkStroke> _pendingDry;

        Dictionary<string, Icon> _icons = new Dictionary<string, Icon>();
        private MainViewModel _dataContextViewModel;

        public MainPage()
        {
            this.InitializeComponent();

            _timer.Tick += async (s, e) => {
                //_inkSync.EndDry();
                _timer.Stop();
                Debug.WriteLine($"finished");
                var boundingBox = GetBoundingBox(_sessionStrokes);
                var bitmap = GetInkBitmap(boundingBox);
                var result = await _dataContextViewModel.ProcessInkImageAsync(bitmap);
                if (!string.IsNullOrWhiteSpace(result.tag))
                    await PlaceIconAsync(result.tag, result.probability, boundingBox);
               
                _sessionStrokes.Clear();
                win2dCanvas.Invalidate();
            };

            

            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += (s, e) =>
            {
                _timer.Stop();
                Debug.WriteLine($"StrokeStarted");
                //ProcessPendingInk();
            };

            inkCanvas.InkPresenter.StrokesCollected += (s, e) => {
                Debug.WriteLine($"StrokeCollected");
                _timer.Stop();
         
                _pendingDry = _inkSync.BeginDry();
                foreach (var stroke in _pendingDry)
                {
                    Debug.WriteLine($"Adding strokes");
                    _sessionStrokes.Add(stroke);
                }

                win2dCanvas.Invalidate();
           
                Debug.WriteLine($"Timer Start");
                _timer.Start();

            };

            _inkSync = inkCanvas.InkPresenter.ActivateCustomDrying();
            
            _dataContextViewModel = this.DataContext as MainViewModel;
            
            this.Unloaded += (s, e) => {
                win2dCanvas.RemoveFromVisualTree();
                win2dCanvas = null;
            };

          
        }

        private async Task PlaceIconAsync(string tag, double probability, Rect boundingBox)
        {
            Debug.WriteLine($"tag: {tag} rating: {probability}");
            if (tag == "other" || probability< 0.50)
            {
                foreach (var stroke in _sessionStrokes)
                    _allStrokes.Add(stroke);
            }
            else
            {
                var icon = await GetIconFileAsync(tag);
                var bytes = await FileIO.ReadBufferAsync(icon);

                var ms = new InMemoryRandomAccessStream();
                var dw = new Windows.Storage.Streams.DataWriter(ms);
                dw.WriteBuffer(bytes);
                await dw.StoreAsync();
                ms.Seek(0);

                var bm = new BitmapImage();
                await bm.SetSourceAsync(ms);

                var wb = new WriteableBitmap(bm.PixelWidth, bm.PixelHeight);
                ms.Seek(0);

                await wb.SetSourceAsync(ms);
                var newBitmap = wb.Resize((int)boundingBox.Width, (int)boundingBox.Height, WriteableBitmapExtensions.Interpolation.Bilinear);
                var image = new Image();
                image.Source = newBitmap;
                Canvas.SetTop(image, boundingBox.Top);
                Canvas.SetLeft(image, boundingBox.Left);
                iconCanvas.Children.Add(image);
            }
        }

        private void Canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            ProcessPendingInk();
            DrawStrokesToInkSurface(args.DrawingSession, _sessionStrokes);
            DrawStrokesToInkSurface(args.DrawingSession, _allStrokes);
            
        }
        private void ProcessPendingInk()
        {
            if (_pendingDry != null)
            {
                _inkSync.EndDry();
                _pendingDry = null;
            }
        }

        private void DrawStrokesToInkSurface(CanvasDrawingSession ds, IList<InkStroke> strokes)
        {
            Debug.WriteLine($"Draw: {strokes.Count}");
            ds.DrawInk(strokes);
        }

        private void ClearInkSurface()
        {
            Debug.WriteLine($"Clear");
            _allStrokes.Clear();
            win2dCanvas.Invalidate();
        }

        private  WriteableBitmap GetInkBitmap(Rect boundingBox)
        {
            WriteableBitmap bitmap = null;
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            using (CanvasRenderTarget offscreen = new CanvasRenderTarget(device, (float)inkCanvas.ActualWidth, (float)inkCanvas.ActualHeight, 96))
            {
                using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                {

                    ds.Units = CanvasUnits.Pixels;
                    ds.Clear(Colors.White);
                    ds.DrawInk(_sessionStrokes);
                }

                var writeableBitmap = new WriteableBitmap((int)offscreen.SizeInPixels.Width, (int)offscreen.SizeInPixels.Height);
                offscreen.GetPixelBytes().CopyTo(writeableBitmap.PixelBuffer);
                bitmap = writeableBitmap.Crop(boundingBox);
            }

            return bitmap;
        }

       
        private Rect GetBoundingBox(IList<InkStroke> strokes)
        {
            Rect _boundingBox = Rect.Empty;
            foreach (var stroke in strokes)
            {
                if (_boundingBox == Rect.Empty)
                    _boundingBox = new Rect(stroke.BoundingRect.X, stroke.BoundingRect.Y, stroke.BoundingRect.Width, stroke.BoundingRect.Height);
                else
                    _boundingBox = _boundingBox.CombineWith(stroke.BoundingRect);
            }
            return _boundingBox;
        }

        private  async Task<IStorageFile> SaveBitmapAsync(WriteableBitmap cropped)
        {
            StorageFolder pictureFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("SmartInkLaboratory", CreationCollisionOption.OpenIfExists);
            var projectFolder = await pictureFolder.CreateFolderAsync(_currentProject, CreationCollisionOption.OpenIfExists);
            var tagFolder = await projectFolder.CreateFolderAsync(_currentTag, CreationCollisionOption.OpenIfExists);
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

            return savefile;
           
        }

        private async Task<IStorageFile> GetIconFileAsync(string tagname)
        {
            var tagVm = ((MainViewModel)this.DataContext).ImageTags;
            var iconVm = ((MainViewModel)this.DataContext).IconMap;

            var tag = await tagVm.GetTagByNameAsync(tagname);
            if (tag != null)
            {
                var icon = await iconVm.GetIconFileAsync(tag.Id);
                return icon;
            }

            return null;
        }

        private async Task<(string tag,float rating)> EvaluateBitmapAsync(WriteableBitmap inkImage)
        {
            SoftwareBitmap evaluate = SoftwareBitmap.CreateCopyFromBuffer(
                    inkImage.PixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    inkImage.PixelWidth,
                    inkImage.PixelHeight
                );

            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(evaluate);
            try
            {
                if (videoFrame != null)
                {
                    ModelInput inputData = new ModelInput();
                    inputData.data = videoFrame;
                    if (_inkModel == null)
                    {
                        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///AI/azureicons.onnx"));
                        _inkModel = await InkModel.CreateModel(file, new Dictionary<string, float>()
                                                                                    {
                                                                                        { "aad", float.NaN },
                                                                                        { "api_app", float.NaN },
                                                                                        { "api_mgmt", float.NaN },
                                                                                        { "asa", float.NaN },
                                                                                        { "bot_services", float.NaN },
                                                                                        { "cosmos_db", float.NaN },
                                                                                        { "event_hub", float.NaN },
                                                                                        { "function", float.NaN },
                                                                                        { "iot_hub", float.NaN },
                                                                                        { "key_vault", float.NaN },
                                                                                        { "other", float.NaN },
                                                                                        { "service_fabric", float.NaN },
                                                                                        { "sql", float.NaN },
                                                                                        { "web_app", float.NaN },
                                                                                         //{ "not_icon", float.NaN },
                        });
                    }
                    var evalOutput = await _inkModel.EvaluateAsync(inputData);
                    
                    return (evalOutput.classLabel[0], evalOutput.loss[evalOutput.classLabel[0]]);
                 
                }
            }
            catch (Exception ex)
            {

                throw;
            }

                return (null,-1);
        }
        private void HyperlinkButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ClearInkSurface();
            iconCanvas.Children.Clear();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
            switch (Pivot.SelectedIndex)
            {
                case 0:
                    _dataContextViewModel.Mode = InteractionMode.Mapping;
                    break;
                case 1:
                    _dataContextViewModel.Mode = InteractionMode.Training;
                    break;
                case 2:
                    _dataContextViewModel.Mode = InteractionMode.Testing;
                    break;
                case 3:
                    _dataContextViewModel.Mode = InteractionMode.Packaging;
                    break;
            }
        }
    }
}
