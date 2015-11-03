using System.Collections.Generic;

namespace Wiki2Dict
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> input, IEnumerable<T> collection)
        {
            var list = input as List<T>;
            if (list != null)
            {
                list.AddRange(collection);
                return;
            }
            foreach (var item in collection)
            {
                input.Add(item);
            }
        }
    }
}