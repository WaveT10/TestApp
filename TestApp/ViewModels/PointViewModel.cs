using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using TestApp.Abstractions;

namespace TestApp
{
    public partial class PointViewModel : ViewModelBase
    {
        private readonly Function _function;
        [ObservableProperty]
        private double? _x;
        [ObservableProperty]
        private double? _y;
        private readonly ObservableCollection<ObservablePoint> _lineSeriesPoints;

        public PointViewModel(Function function, int pointNumber, ObservableCollection<ObservablePoint> lineSeriesPoints)
        {
            _function = function;
            Number = pointNumber;
            _lineSeriesPoints = lineSeriesPoints;
            var point = _function.GetPointOrDefault(Number);
            X = point?.X;
            Y = point?.Y;
        }

        public int Number { get; private set; }

        public bool IsInitialized => X.HasValue && Y.HasValue;

        partial void OnXChanged(double? value) 
        {
            UpdatePoint();
        }

        partial void OnYChanged(double? value)
        {
            UpdatePoint();
        }

        private void UpdatePoint()
        {
            if (!IsInitialized)
            {
                return;
            }

            var newPoint = new Point(Number, X!.Value, Y!.Value);
            var oldPoint = _function.GetPointOrDefault(Number);

            if (oldPoint is null)
            {
                _function.AddPoint(newPoint);
                _lineSeriesPoints.Add(new ObservablePoint(newPoint.X, newPoint.Y));
            }
            else
            {
                var pointIndex = _function.GetPointIndex(Number);
                var lineSeriesPoint = _lineSeriesPoints[pointIndex];

                lineSeriesPoint.X = newPoint.X;
                lineSeriesPoint.Y = newPoint.Y;
                _function.UpdatePoint(newPoint);
            }
        }
    }
}
