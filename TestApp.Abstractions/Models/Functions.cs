namespace TestApp.Abstractions
{
    public sealed class Functions : IEquatable<Functions>
    {
        private List<Function> _list = new List<Function>();

        public Functions() 
        {
        }

        public Functions(IReadOnlyCollection<Function> list)
        {
            _list = list.ToList();
        }

        public IReadOnlyCollection<Function> List => _list;

        public bool AllFunctionsEmpty => List.All(function => function.Empty);

        private int LastFunctionNumber => List.MaxBy(point => point.Number)?.Number ?? 0;

        public Function AddFunction() 
        {
            var function = new Function([], LastFunctionNumber + 1);

            _list.Add(function);
            return function;
        }

        public Function AddInversedFunction(int originalFunctionNumber)
        {
            var function = GetFunction(originalFunctionNumber);
            var inversedFunction = function.ToInversedFunction(inversedFunctionNumber: LastFunctionNumber + 1);
            _list.Add(inversedFunction);
            return inversedFunction;
        }

        public void Remove(int functionNumber)
        {
            var function = GetFunction(functionNumber);

            _list.Remove(function);
        }

        public Function GetFunction(int functionNumber)
        {
            return _list.SingleOrResourceNotFound(function => function.Number == functionNumber);
        }

        public Function? GetLastFunctionWithNearPoint(double x, double y, double acceptedDistance)
        {
            return _list.Where(function => function.GetLastNearPoint(x, y, acceptedDistance) is not null)
                        .OrderByDescending(function => function.Number)
                        .MinBy(function => function.GetLastNearPoint(x, y, acceptedDistance)?.GetDistanceTo(x, y));
        }

        public void Reassign(IReadOnlyCollection<Function> functions)
        {
            var hasSameNumbers = functions.GroupBy(point => point.Number).Any(group => group.Count() > 1);

            if (hasSameNumbers)
            {
                throw new BadArgumentException(Errors.ItemWithTheSameNumberExists);
            }

            _list = functions.ToList();
        }

        public Functions Clone()
        {
            return new Functions(List.Select(function => function.Clone()).ToList());
        }

        public bool Equals(Functions? other)
        {
            return other is not null && new HashSet<Function>(List).SetEquals(other.List);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Functions);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var function in List)
            {
                hash.Add(function);
            }

            return hash.ToHashCode();
        }
    }
}
