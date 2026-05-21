namespace TestApp.Abstractions
{
    public sealed class Point : IEquatable<Point>
    {
        public Point(int number, double x, double y)
        {
            Number = number;
            X = x;
            Y = y;
        }

        public int Number { get; }

        public double X { get; private set; }
        
        public double Y { get; private set; }

        public void Update(double x, double y) 
        {
            X = x;
            Y = y;
        }

        public double GetDistanceTo(double x, double y)
        {
            return Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));
        }

        public Point Clone() 
        {
            return new Point(Number, X, Y);
        }

        public Point ToInversedPoint() 
        {
            return new Point(Number, x: Y, y: X);
        }

        public bool Equals(Point? other)
        {
            return other is not null && other.Number == Number && other.X == X && other.Y == Y;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Point);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Number, X, Y);
        }
    }
}
