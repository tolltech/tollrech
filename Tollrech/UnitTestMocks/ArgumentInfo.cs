using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Tollrech.UnitTestMocks
{
    public class ArgumentInfo
    {
        public IType Type { get; set; }
        public ICSharpExpression Expression { get; set; }
        public bool IsCSharpArgument { get; set; }
    }
}