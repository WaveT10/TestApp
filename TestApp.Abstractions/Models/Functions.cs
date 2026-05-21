namespace TestApp.Abstractions
{
    public sealed class Functions : IEquatable<Functions>
    {
        private Dictionary<int, Function> _functionsDictionary = new Dictionary<int, Function>();

        public Functions()
        {
        }

        public Functions(IReadOnlyCollection<Function> list)
        {
            _functionsDictionary = list.ToDictionaryOrThrowIfDupplicate(function => function.Number);
        }

        public IReadOnlyCollection<Function> List => _functionsDictionary.Values;

        public bool AllFunctionsEmpty => List.All(function => function.Empty);

        private int LastFunctionNumber => List.MaxBy(point => point.Number)?.Number ?? 0;

        public Function AddFunction()
        {
            var function = new Function([], LastFunctionNumber + 1);

            _functionsDictionary[function.Number] = function;
            return function;
        }

        public Function AddInversedFunction(int originalFunctionNumber)
        {
            var function = _functionsDictionary.GetOrResourceNotFound(originalFunctionNumber);
            var inversedFunction = function.ToInversedFunction(inversedFunctionNumber: LastFunctionNumber + 1);
            _functionsDictionary[inversedFunction.Number] = inversedFunction;
            return inversedFunction;
        }

        public void Remove(int functionNumber)
        {
            _functionsDictionary.RemoveOrResourceNotFound(functionNumber);
        }

        public Function? GetLastFunctionWithNearPoint(double x, double y, double acceptedDistance)
        {
            return List.Where(function => function.GetLastNearPoint(x, y, acceptedDistance) is not null)
                       .OrderByDescending(function => function.Number)
                       .MinBy(function => function.GetLastNearPoint(x, y, acceptedDistance)?.GetDistanceTo(x, y));
        }

        public void Reassign(IReadOnlyCollection<Function> functions)
        {
            _functionsDictionary = functions.ToDictionaryOrThrowIfDupplicate(function => function.Number);
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
