using System;
using JetBrains.Util;

namespace Tollrech.Common
{
    public static class StringExtensions
    {
        public static string MorphemToManies(this string src)
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

        public static string MakeFirsCharLowercase(this string src)
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