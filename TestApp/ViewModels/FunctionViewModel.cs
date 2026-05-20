using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TestApp.Services;
using Avalonia.Input.Platform;
using TestApp.Abstractions;
using Point = TestApp.Abstractions.Point;
using System.Drawing;

namespace TestApp
{
    public partial class FunctionViewModel : ViewModelBase
    {
        private readonly IClipboard? _clipboard;
        private readonly IModelConverterFactory _modelConverterFactory;
        private Function _function;
        private ObservableCollection<ObservablePoint> _lineSeriesPoints;        
        [ObservableProperty]
        private bool _allowNewPoint = true;
        [ObservableProperty]
        private ObservableCollection<PointViewModel> _points;
        [ObservableProperty]
        private bool _isStrictlyMonotonic;

        public FunctionViewModel(Function function, IClipboard? clipboard, IModelConverterFactory modelConverterFactory)
        {
            _modelConverterFactory = modelConverterFactory;
            _clipboard = clipboard;
            _function = function;
            LineSeries = new LineSeries<ObservablePoint>
            {
                Fill = null,
                GeometrySize = 10,
                LineSmoothness = 0
            };
            Assign(function);
            _clipboard = clipboard;
            _modelConverterFactory = modelConverterFactory;
        }        

        public int Number => _function.Number;

        public LineSeries<ObservablePoint> LineSeries { get; private set; }

        private bool IsNewPointAllowed => !Points.Any() || Points.LastOrDefault()?.IsInitialized == true;

        public void UpdatePoint(Point oldPoint, double x, double y, double acceptedDistance) 
        {
            var pointViewModel = Points.Single(point => point.Number == oldPoint.Number);

            pointViewModel.X = x;
            pointViewModel.Y = y;
        }

        [RelayCommand]
        private void RemovePoint(int pointNumber)
        {
            var pointViewModel = Points.Single(point => point.Number == pointNumber);            

            if (_function.Exists(pointNumber))
            {
                var pointIndex = _function.GetPointIndex(pointNumber);
                _function.RemovePoint(pointNumber);                
                _lineSeriesPoints.Remove(_lineSeriesPoints[pointIndex]);
            }

            Points.Remove(pointViewModel);
        }

        [RelayCommand]
        private void AddPoint()
        {
            var maxPointNumber = Points.MaxBy(point => point.Number)?.Number ?? 0;
            var point = new PointViewModel(_function, ++maxPointNumber, _lineSeriesPoints);
            point.PropertyChanged += (s, e) => RecalculateProperties();
            Points.Add(point);
        }

        [RelayCommand]
        private async Task PasteFromClipboard()
        {
            if (_clipboard == null) 
            {
                return;
            }

            var text = await _clipboard.TryGetTextAsync();
            _modelConverterFactory.Get(ContentTypes.TSV).ToFunction(_function, text);
            Assign(_function);
        }

        [RelayCommand]
        private async Task CopyToClipboard()
        {
            if (_clipboard == null)
            {
                return;
            }

            var text = _modelConverterFactory.Get(ContentTypes.TSV).FromFunction(_function);
            await _clipboard.SetTextAsync(text);
        }

        private void Assign(Function function)
        {
            var points = function.Points.OrderBy(point => point.Number);
            var lineSeriesPoints = new ObservableCollection<ObservablePoint>(points.Select(point => new ObservablePoint(point.X, point.Y)));
            _lineSeriesPoints = lineSeriesPoints;
            LineSeries.Values = _lineSeriesPoints;
            Points = new ObservableCollection<PointViewModel>(points.Select(point => new PointViewModel(function, point.Number, _lineSeriesPoints)));
            IsStrictlyMonotonic = function.IsStrictlyMonotonic;
            Points.CollectionChanged += (s, e) => RecalculateProperties();

            foreach (var point in Points)
            {
                point.PropertyChanged += (s, e) => RecalculateProperties();
            }
        }

        private void RecalculateProperties() 
        {
            AllowNewPoint = IsNewPointAllowed;
            IsStrictlyMonotonic = _function.IsStrictlyMonotonic;
        }
    }
}
