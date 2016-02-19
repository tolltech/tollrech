using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech
{
    [ContextAction(Group = "C#", Name = "UnitTestCreate Action", Description = "Create unit test")]
    public class UnitTestCreator : ContextActionBase
    {
        public ICSharpContextActionDataProvider Provider { get; set; }

        public UnitTestCreator(ICSharpContextActionDataProvider provider)
        {
            Provider = provider;
        }

        public override string Text { get; } = "Create mocked class";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            var method = Provider.GetSelectedElement<IMethodDeclaration>();
            if (method == null)
                return false;

            var ctor = Provider.GetSelectedElement<IConstructorInitializer>(false);
            if (ctor == null)
                return false;

            return true;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            ReplaceType();

            return null;
        }

        private void ReplaceType()
        {
            var ctor = Provider.GetSelectedElement<IConstructorInitializer>(false);
            var type = ctor?.ConstructedType;
            if (ctor == null || type == null)
                return;

            var typePresentableName = type.GetPresentableName(CSharpLanguage.Instance);

            var factory = CSharpElementFactory.GetInstance(Provider.PsiModule);
            var ctorArgs = ctor.ArgumentList;

            foreach (var argument in ctorArgs.Arguments)
            {
                var code = $"NewMock<>()";

                var newExpression = factory.CreateExpression(code);
                argument.SetValue(newExpression);
            }

            ctor.SetArgumentList(ctorArgs);

        }
    }

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

                var argExpression = factory.CreateExpression($"NewMock<$0>();", ctorParam.Type);

                if (classDeclaration != null)
                {
                    classDeclaration.AddClassMemberDeclaration(factory.CreateFieldDeclaration(ctorParam.Type, ctorParam.ShortName));
                    var ctorStatement = methodDeclaration.Body.Statements.FirstOrDefault(x => (x as IExpressionStatement)?.Expression == ctorExpression);
                    methodDeclaration.Body.AddStatementBefore(factory.CreateStatement("$0 = $1;", ctorParam.ShortName, argExpression), ctorStatement);
                }
            }

            var names = ctorParams.Select(x => x.Type.IsInterfaceType() ? x.ShortName : "TODO");
            var argumentsPattern = string.Join(", ", Enumerable.Range(1, ctorParams.Count).Select(x => string.Format("${0}", x)));
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
            var classDeclaration = ctorTreeNode?.FindParent<IClassDeclaration>();
            if (classDeclaration == null)
                return false;
            return true;
            var methods = classDeclaration.SuperTypes.SelectMany(x => x.GetTypeElement()?.Methods);
            return methods.Any(x => x.ShortName == "NewMock");
        }
    }
}