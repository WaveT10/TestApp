using System.Linq;

namespace TestApp.Abstractions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrResourceNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) 
        {
            return dictionary.TryGetValue(key, out var value) ? value : throw new BadArgumentException(Errors.ResourceNotFound);
        }

        public static void RemoveOrResourceNotFound<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (!dictionary.Remove(key))
            {
                throw new BadArgumentException(Errors.ResourceNotFound);
            }
        }

        public static Dictionary<TKey, TValue> ToDictionaryOrThrowIfDupplicate<TKey, TValue>(this IReadOnlyCollection<TValue> list, Func<TValue, TKey> keySelector) where TKey : notnull
        {
            var groups = list.GroupBy(keySelector);

            if (groups.Any(group => group.Count() > 1)) 
            {
                throw new BadArgumentException(Errors.ItemWithTheSameNumberExists);
            }

            return groups.ToDictionary(group => group.Key, group => group.Single());
        }
    }
}
