using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech
{
    [QuickFix]
    public class MockedClassCreateFix : QuickFixBase
    {
        private readonly IncorrectArgumentNumberError error;

        public MockedClassCreateFix(IncorrectArgumentNumberError error)
        {
            this.error = error;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var ctorTreeNode = error?.Reference.GetTreeNode();
            var ctorExpression = ctorTreeNode as IObjectCreationExpression;
            var ctor = error?.Reference?.CurrentResolveResult?.DeclaredElement as IConstructor;
            if (ctor == null || ctorExpression == null)
                return null;

            var ctorParams = ctor.Parameters;
            if (ctorParams.Count == 0)
                return null;

            var factory = CSharpElementFactory.GetInstance(error.Reference.GetAccessContext().GetPsiModule());

            var methodDeclaration = ctorTreeNode.FindParent<IMethodDeclaration>();
            var classDeclaration = methodDeclaration?.FindParent<IClassDeclaration>();
            foreach (var ctorParam in ctorParams)
            {
                if (!ctorParam.Type.IsInterfaceType())
                    continue;

                var argExpression = factory.CreateExpression("NewMock<$0>();", ctorParam.Type);

                if (classDeclaration != null)
                {
                    classDeclaration.AddClassMemberDeclaration(factory.CreateFieldDeclaration(ctorParam.Type, ctorParam.ShortName));
                    var ctorStatement = methodDeclaration.Body.Statements.FirstOrDefault(x => (x as IExpressionStatement)?.Expression == ctorExpression);
                    methodDeclaration.Body.AddStatementBefore(factory.CreateStatement("$0 = $1;", ctorParam.ShortName, argExpression), ctorStatement);
                }
            }

            var names = ctorParams.Select(x => x.Type.IsInterfaceType() ? x.ShortName : "TODO");
            var argumentsPattern = string.Join(", ", Enumerable.Range(1, ctorParams.Count).Select(x => $"${x}"));
            var objArgExpressions = new object[] { ctor.GetContainingType() }.Concat(names).ToArray();
            var newExpression = factory.CreateExpression($"new $0({argumentsPattern});", objArgExpressions);

            ctorExpression.ReplaceBy(newExpression);

            return null;
        }

        public override string Text => "Create mocked class";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(error?.Reference?.CurrentResolveResult?.DeclaredElement is IConstructor))
                return false;

            var ctorTreeNode = error?.Reference.GetTreeNode();
            var classDeclaration = ctorTreeNode.FindParent<IClassDeclaration>();
            if (classDeclaration == null)
                return false;

            var superTypes = classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes);
            return superTypes.Any(x => x.GetClassType()?.Methods.Any(y => y.ShortName == "NewMock") ?? false);
        }
    }
}