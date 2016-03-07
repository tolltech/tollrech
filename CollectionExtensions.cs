using System;
using System.Collections.Generic;

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
    }
}