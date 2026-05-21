namespace TestApp.Abstractions
{
    public sealed class Function : IEquatable<Function>
    {
        private Dictionary<int, Point> _pointsDictionary;

        public Function(IReadOnlyCollection<Point> points, int number)
        {
            _pointsDictionary = points.ToDictionaryOrThrowIfDupplicate(point => point.Number);
            Number = number;
        }

        public int Number { get; }

        public IReadOnlyCollection<Point> Points => _pointsDictionary.Values;

        public bool Empty => !_pointsDictionary.Any();

        public int LastPointNumber => _pointsDictionary.Keys.DefaultIfEmpty().Max();

        public bool IsStrictlyMonotonic 
        {
            get 
            {
                if (_pointsDictionary.Count < 2) 
                {
                    return false;
                }


                Point? previousPoint = null;
                double? previousDeltaX = null;
                double? previousDeltaY = null;

                foreach (var point in Points.OrderBy(point => point.Number)) 
                {
                    if (previousPoint == null) 
                    {
                        previousPoint = point;
                        continue;
                    }

                    var deltaX = point.X - previousPoint.X;
                    var deltaY = point.Y - previousPoint.Y;

                    if (deltaX == 0 || deltaY == 0) 
                    {
                        return false;
                    }

                    var isNotStrictlyMonotonic = Math.Sign(previousDeltaX.GetValueOrDefault()) != Math.Sign(deltaX) ||
                                                 Math.Sign(previousDeltaY.GetValueOrDefault()) != Math.Sign(deltaY);

                    if (previousDeltaX is not null && previousDeltaY is not null && isNotStrictlyMonotonic) 
                    {
                        return false;                    
                    }

                    previousDeltaX = deltaX;
                    previousDeltaY = deltaY;
                    previousPoint = point;
                }

                return true;
            }
        }

        public void AddPoint(Point point)
        {
            if (_pointsDictionary.ContainsKey(point.Number)) 
            {
                throw new BadArgumentException(Errors.ItemWithTheSameNumberExists);
            }

            _pointsDictionary[point.Number] = point;
        }

        public void UpdatePoint(Point newPoint)
        {
            _pointsDictionary.GetOrResourceNotFound(newPoint.Number).Update(newPoint.X, newPoint.Y);
        }

        public void RemovePoint(int pointNumber)    
        {
            _pointsDictionary.RemoveOrResourceNotFound(pointNumber);
        }

        public Point? GetPointOrDefault(int pointNumber)
        {
            return _pointsDictionary.TryGetValue(pointNumber, out var point) ? point : null;
        }

        public int GetPointIndex(int pointNumber)
        {
            var point = _pointsDictionary.GetOrResourceNotFound(pointNumber);

            return Points.OrderBy(point => point.Number).ToList().IndexOf(point);
        }

        public bool Exists(int pointNumber)
        {
            return _pointsDictionary.ContainsKey(pointNumber);
        }

        public Point? GetLastNearPoint(double x, double y, double acceptedDistance) 
        {
            return Points.Where(point => point.GetDistanceTo(x, y) < acceptedDistance)
                         .OrderByDescending(point => point.Number)
                         .MinBy(point => point.GetDistanceTo(x, y));
        }

        public void Reassign(IReadOnlyCollection<Point> points)
        {
            _pointsDictionary = points.ToDictionaryOrThrowIfDupplicate(point => point.Number);
        }

        public Function ToInversedFunction(int inversedFunctionNumber) 
        {
            var inversedPoints = Points.Select(point => point.ToInversedPoint()).ToList();
            return IsStrictlyMonotonic ? 
                new Function(inversedPoints, inversedFunctionNumber) : 
                throw new InvalidOperationException(Errors.CanNotConvertToInvercedFunction);
        }

        public Function Clone()
        {
            return new Function(Points.Select(point => point.Clone()).ToList(), Number);
        }

        public bool Equals(Function? other)
        {
            return other is not null && other.Number == Number && new HashSet<Point>(Points).SetEquals(other.Points);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Function);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var point in Points)
            {
                hash.Add(point);
            }

            hash.Add(Number);
            return hash.ToHashCode();
        }
    }
}
