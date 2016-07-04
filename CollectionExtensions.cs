using System;
using System.Collections.Generic;
using System.Linq;

namespace Tollrech
{
    public static class CollectionExtensions
    {
        public static T InfinitivePop<T>(this IList<T> list)
        {
            if (list.Count == 0)
                throw new IndexOutOfRangeException();

            var result = list[0];

            if (list.Count > 1)
                list.RemoveAt(0);

            return result;
        }

        public static T Pop<T>(this IList<T> list, Func<T, bool> cond) where T : class
        {
            if (list.Count == 0)
                return null;

            var result = list.FirstOrDefault(cond);

            if (result != null)
            {
                list.Remove(result);
            }

            return result;
        }
    }
}