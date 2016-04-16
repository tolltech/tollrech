using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Tollrech.UnitTestMocks
{
    public struct MockInfo
    {
        public IType Type { get; set; }
        public string Name { get; set; }
        public ICSharpStatement Statement { get; set; }
    }
}