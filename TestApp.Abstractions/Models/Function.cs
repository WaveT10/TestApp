namespace TestApp.Abstractions
{
    public sealed class Function : IEquatable<Function>
    {
        private List<Point> _points;

        public Function(IReadOnlyCollection<Point> points, int number)
        {
            _points = points.ToList();
            Number = number;
        }

        public int Number { get; }

        public IReadOnlyCollection<Point> Points => _points;

        public bool Empty => !_points.Any();

        public bool IsStrictlyMonotonic 
        {
            get 
            {
                if (_points.Count < 2) 
                {
                    return false;
                }


                Point? previousPoint = null;
                double? previousDeltaX = null;
                double? previousDeltaY = null;

                foreach (var point in _points) 
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
            if (_points.Any(p => p.Number == point.Number)) 
            {
                throw new BadArgumentException(Errors.ItemWithTheSameNumberExists);
            }

            _points.Add(point);
        }

        public Point UpdatePoint(Point newPoint)
        {
            return GetPoint(newPoint.Number).Update(newPoint.X, newPoint.Y);
        }

        public void RemovePoint(int pointNumber)    
        {
            var point = GetPoint(pointNumber);

            _points.Remove(point);
        }

        public Point? GetPointOrDefault(int pointNumber)
        {
            return _points.SingleOrDefault(point => point.Number == pointNumber);
        }

        public int GetPointIndex(int pointNumber)
        {
            var point = GetPoint(pointNumber);

            return _points.OrderBy(point => point.Number).ToList().IndexOf(point);
        }

        public bool Exists(int pointNumber)
        {
            return GetPointOrDefault(pointNumber) is not null;
        }

        public Point? GetLastNearPoint(double x, double y, double acceptedDistance) 
        {
            return _points.Where(point => point.GetDistanceTo(x, y) < acceptedDistance)
                          .OrderByDescending(point => point.Number)
                          .MinBy(point => point.GetDistanceTo(x, y));
        }

        public void Reassign(IReadOnlyCollection<Point> points)
        {
            var hasSameNumbers = points.GroupBy(point => point.Number).Any(group => group.Count() > 1);

            if (hasSameNumbers) 
            {
                throw new BadArgumentException(Errors.ItemWithTheSameNumberExists);
            }

            _points = points.ToList();
        }

        public Function ToInversedFunction(int inversedFunctionNumber) 
        {
            var inversedPoints = _points.Select(point => point.ToInversedPoint()).ToList();
            return IsStrictlyMonotonic ? 
                new Function(inversedPoints, inversedFunctionNumber) : 
                throw new InvalidOperationException(Errors.CanNotConvertToInvercedFunction);
        }

        public Function Clone()
        {
            return new Function(_points.Select(point => point.Clone()).ToList(), Number);
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

        private Point GetPoint(int pointNumber)
        {
            return _points.SingleOrResourceNotFound(point => point.Number == pointNumber);
        }
    }
}
