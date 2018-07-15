using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Tollrech.Common
{
    public static class SyntaxExtensions
    {
        [CanBeNull]
        public static IAttribute FindAttriobute([NotNull] this IEnumerable<IAttribute> src, string name)
        {
            return src.FirstOrDefault(x => x.Name.NameIdentifier.Name == name);
        }

        public static bool HasAttriobute([NotNull] this IEnumerable<IAttribute> src, string name)
        {
            return src.Any(x => x.Name.NameIdentifier.Name == name);
        }

        [CanBeNull]
        public static string GetLiteralText([CanBeNull] this ICSharpArgument src)
        {
            return (src?.Expression as ICSharpLiteralExpression)?.Literal.GetText().Trim('"');
        }
    }
}