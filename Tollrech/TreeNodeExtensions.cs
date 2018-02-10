using JetBrains.ReSharper.Psi.Tree;

namespace Tollrech
{
    public static class TreeNodeExtensions
    {
        public static T FindParent<T>(this ITreeNode node)
        {
            while (true)
            {
                if (node.Parent == null)
                {
                    return default(T);
                }

                if (node.Parent is T)
                {
                    return (T) node.Parent;
                }

                node = node.Parent;
            }
        }
    }
}