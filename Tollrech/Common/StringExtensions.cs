using System;
using JetBrains.Annotations;
using JetBrains.Util;

namespace Tollrech.Common
{
    public static class StringExtensions
    {
        [NotNull]
        public static string MorphemToManies([NotNull] this string src)
        {
            if (src.EndsWith("Dbo", StringComparison.InvariantCultureIgnoreCase))
            {
                src = src.TrimFromEnd("Dbo", StringComparison.InvariantCultureIgnoreCase);
            }

            if (src.EndsWith("s") || src.EndsWith("o"))
            {
                return $"{src}es";
            }

            if (src.EndsWith("y"))
            {
                return $"{src.TrimEnd('y')}ies";
            }

            return $"{src}s";
        }

        [NotNull]
        public static string MakeFirsCharLowercase([NotNull] this string src)
        {
            if (src.Length < 1)
            {
                return src;
            }

            if (src.Length == 1)
            {
                return src.ToLower();
            }

            return $"{src[0].ToString().ToLower()}{src.Substring(1, src.Length - 1)}";
        }
    }
}