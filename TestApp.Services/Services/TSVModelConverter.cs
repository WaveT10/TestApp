using TestApp.Abstractions;

namespace TestApp.Services
{
    internal sealed class TSVModelConverter : IModelConverter
    {
        public void ToFunction(Function funtion, string? text, bool overwriteIfEmptyText = false)
        {
            var rows = GetRows(text);
            var points = new List<Point>();

            foreach (var row in rows)
            {
                var columns = GetColumns(row);

                if (int.TryParse(columns[0], out var number) &&
                    double.TryParse(columns[1], out var x) &&
                    double.TryParse(columns[2], out var y)) 
                {
                    var point = new Point(number, x, y);

                    points.Add(point);
                }
            }

            if (points.Any() || overwriteIfEmptyText)
            {
                funtion.Reassign(points);
            }
        }

        public void ToFunctions(Functions functions, string? text, bool overwriteIfEmptyText = false)
        {
            var rows = GetRows(text);
            var functionsPoints = new Dictionary<int, List<Point>>();

            foreach (var row in rows)
            {
                var columns = GetColumns(row);

                if (int.TryParse(columns[0], out var functionNumber) &&
                    int.TryParse(columns[1], out var pointNumber) &&
                    double.TryParse(columns[2], out var x) &&
                    double.TryParse(columns[3], out var y))
                {
                    var point = new Point(pointNumber, x, y);

                    if (functionsPoints.TryGetValue(functionNumber, out var points))
                    {
                        points.Add(point);
                        continue;
                    }

                    functionsPoints[functionNumber] = [point];
                }
            }

            if (functionsPoints.Any() || overwriteIfEmptyText)
            {
                var functionList = functionsPoints.Select(functionPoints => new Function(points: functionPoints.Value,
                                                                                         number: functionPoints.Key))
                                                  .ToList();
                functions.Reassign(functionList);
            }
        }

        public string FromFunction(Function function)
        {
            return string.Join(Environment.NewLine, 
                               function.Points.Select(point => GetTSVLine(point.Number, point.X, point.Y)));
        }

        public string FromFunctions(Functions functions)
        {
            return string.Join(Environment.NewLine,
                               functions.List.SelectMany(function => 
                                   function.Points.Select(point =>
                                       GetTSVLine(function.Number, point.Number, point.X, point.Y))));
        }

        private static string GetTSVLine(params object[] values) 
        {
            return string.Join('\t', values);
        }

        private static string[] GetRows(string? text) 
        {
            return (text ?? string.Empty).Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        }

        private static string[] GetColumns(string row)
        {
            return row.Split("\t");
        }
    }
}
