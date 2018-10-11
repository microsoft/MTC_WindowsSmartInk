using AMP.Views;
using EBC_InkDemo.Extensions;
using EBC_InkDemo.ViewModels;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EBC_InkDemo.Views
{

    internal struct Icon
    {
        public string Id { get; private set; }
        public string Path { get; private set; }
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
        PackageManager _packageManager = new PackageManager();
        private MainViewModel _dataContextViewModel;

        DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(750) };
        List<InkStroke> _sessionStrokes = new List<InkStroke>();
        List<InkStroke> _allStrokes = new List<InkStroke>();

        private InkSynchronizer _inkSync;
        IReadOnlyList<InkStroke> _pendingDry;

        Dictionary<string, Icon> _icons = new Dictionary<string, Icon>();
        
        public MainPage()
        {
            this.InitializeComponent();
            _dataContextViewModel = this.DataContext as MainViewModel;
            _timer.Tick += async (s, e) => {
                _timer.Stop();
                var boundingBox = GetBoundingBox(_sessionStrokes);
                try
                {

                    var result = await _dataContextViewModel.ProcessInkAsync(_sessionStrokes);
                    if (result != null && result.Keys.Count > 0)
                    {
                        var top = (from r in result select r).First();
                        await PlaceIconAsync(top.Key, top.Value, boundingBox);
                    }

                }
                catch (Exception ex)
                {


                }
                finally
                {
                    _sessionStrokes.Clear();
                    win2dCanvas.Invalidate();
                }
            };

            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += (s, e) =>
            {
                _timer.Stop();
            };

            inkCanvas.InkPresenter.StrokesCollected += (s, e) => {
                _timer.Stop();

                _pendingDry = _inkSync.BeginDry();
                foreach (var stroke in _pendingDry)
                    _sessionStrokes.Add(stroke);

                win2dCanvas.Invalidate();

                _timer.Start();
            };

            _inkSync = inkCanvas.InkPresenter.ActivateCustomDrying();

            this.Unloaded += (s, e) => {
                win2dCanvas.RemoveFromVisualTree();
                win2dCanvas = null;
            };
        }

        private async Task PlaceIconAsync(string tag, double probability, Rect boundingBox)
        {
            if (probability < 0.4)
            {
                foreach (var stroke in _sessionStrokes)
                    _allStrokes.Add(stroke);
            }
            else
            {
                var icon = await _dataContextViewModel.GetIconAsync(tag);
                if (icon == null)
                    return;

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

        private void listviewInstalledPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
                vm.PackageSelected.Execute(listviewInstalledPackages.SelectedItem);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            _allStrokes.Clear();
            win2dCanvas.Invalidate();
            iconCanvas.Children.Clear();
        }

        private void WelcomeGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ShowWelcome();
        }

        private void ShowWelcome()
        {
            VisualStateManager.GoToState(this, "Active", true);
        }

        private void win2dCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
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
            ds.DrawInk(strokes);
        }

    }
}
