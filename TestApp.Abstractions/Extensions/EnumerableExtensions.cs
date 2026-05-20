namespace TestApp.Abstractions
{
    internal static class EnumerableExtensions
    {
        public static T SingleOrResourceNotFound<T>(this IEnumerable<T> source, Func<T, bool> predicate) 
        {
            return source.SingleOrDefault(predicate) ?? throw new BadArgumentException(Errors.ResourceNotFound);
        }
    }
}
