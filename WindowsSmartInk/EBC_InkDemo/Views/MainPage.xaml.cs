using AMP.Views;
using EBC_InkDemo.Extensions;
using EBC_InkDemo.ViewModels;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        DispatcherTimer _inactiveTimer = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(3) };
        DispatcherTimer _inkTimer;
        List<InkStroke> _sessionStrokes = new List<InkStroke>();
        List<InkStroke> _allStrokes = new List<InkStroke>();

        private InkSynchronizer _inkSync;
        IReadOnlyList<InkStroke> _pendingDry;

        Dictionary<string, Icon> _icons = new Dictionary<string, Icon>();
        
        public MainPage()
        {
            this.InitializeComponent();
            _dataContextViewModel = this.DataContext as MainViewModel;
            _inkTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(_dataContextViewModel.InkProcessingDelay) };
            _dataContextViewModel.InkProcessingDelayChanged += (s, e) => {
                Debug.WriteLine($"InkDelayChanged");
                _inkTimer.Stop();
                _inkTimer.Interval = TimeSpan.FromMilliseconds(_dataContextViewModel.InkProcessingDelay);
                ClearCanvas();
            };
            _inactiveTimer.Tick += (s, e) => { ShowWelcome(); };
            _inkTimer.Tick += async (s, e) => {
                _inkTimer.Stop();
                var boundingBox = GetBoundingBox(_sessionStrokes);
                try
                {

                    var result = await _dataContextViewModel.ProcessInkAsync(_sessionStrokes);
                    if (result != null && result.Keys.Count > 0)
                    {
                        var top = (from r in result select r).First();
                        UpdateAIStats(result);
                        await PlaceIconAsync(top.Key, top.Value, boundingBox);
                    }

                }
                catch (Exception ex)
                {


                }
                finally
                {
                    UpdateInkStats(_sessionStrokes);
                    _sessionStrokes.Clear();
                    win2dCanvas.Invalidate();
                }
            };

            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += (s, e) =>
            {
                _inkTimer.Stop();
                _inactiveTimer.Stop();
            };

            inkCanvas.InkPresenter.StrokesCollected += (s, e) => {
                _inkTimer.Stop();
                _inactiveTimer.Stop();

                _pendingDry = _inkSync.BeginDry();
                foreach (var stroke in _pendingDry)
                    _sessionStrokes.Add(stroke);

                win2dCanvas.Invalidate();

                _inkTimer.Start();
                _inactiveTimer.Start();
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

    
        private void WelcomeGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            HideWelcome();
        }

        private void HideWelcome()
        {
            VisualStateManager.GoToState(this, "Active", true);
            _inactiveTimer.Start();
        }

        private void ShowWelcome()
        {
            VisualStateManager.GoToState(this, "NotActive", true);
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

        private void buttonDelete_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ClearCanvas();
        }

        public void UpdateInkStats(IList<InkStroke> strokes)
        {
            var strokeCount = strokes.Count;
            var segmentCount = (from s in strokes select s.GetRenderingSegments().Count).Sum();
            var pointCount = (from s in strokes select s.GetInkPoints().Count).Sum();
            var totalTime = (from s in strokes where s.StrokeDuration.HasValue select  s.StrokeDuration.Value.TotalSeconds).Sum();


            textInkStats.Text = string.Empty;
            var builder = new StringBuilder();
            builder.AppendLine($"Total Strokes: {strokeCount}");
            builder.AppendLine($"Total Segments: {segmentCount}");
            builder.AppendLine($"Total Ink Points: {pointCount}");
            builder.AppendLine($"Total Time: {totalTime}s");

            textInkStats.Text = builder.ToString();
        }

        private void UpdateAIStats(IDictionary<string, float> result)
        {
            textAIStats.Text = string.Empty;
            var builder = new StringBuilder();

            var kvps = (from r in result.AsEnumerable() select r).Take(5);
            foreach (var kvp in kvps)
                builder.AppendLine($"{kvp.Key} - {kvp.Value}");

            textAIStats.Text = builder.ToString();

        }

        private void ClearCanvas()
        {
            _allStrokes.Clear();
            _sessionStrokes.Clear();
            win2dCanvas.Invalidate();
            iconCanvas.Children.Clear();
        }
    }
}
