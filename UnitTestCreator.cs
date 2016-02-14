using System;
using System.Collections.Generic;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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
            var ctorExpression = error?.Reference.GetTreeNode() as IObjectCreationExpression;//IObjectCreationExpressiotn.Arguments = 0
            var ctor = error?.Reference?.CurrentResolveResult?.DeclaredElement as IConstructor;// тут инфа о параметрах
            if (ctor == null || ctorExpression == null)
                return null;

            var ctorParams = ctor.Parameters;

            var factory = CSharpElementFactory.GetInstance(error.Reference.GetAccessContext().GetPsiModule());

            var argExpressions = new List<ICSharpExpression>();
            foreach (var ctorParam in ctorParams)
            {
                var argExpression = factory.CreateExpression("NewMock<$0>()", ctorParam.GetContainingType());
                argExpressions.Add(argExpression);
            }

            var newExpression = factory.CreateExpression("new $0($1)", ctor.GetContainingType(), argExpressions[0]);
            ctorExpression.ReplaceBy(newExpression);

            return null;
        }

        public override string Text => "Create mocked class";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return error?.Reference?.CurrentResolveResult?.DeclaredElement is IConstructor;
        }
    }

    [QuickFix]
    public class TrashFix : QuickFixBase
    {
        private readonly IncorrectArgumentNumberError error;

        public TrashFix(IncorrectArgumentNumberError error)
        {
            this.error = error;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            return null;
        }

        public override string Text => "Tr";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return false;
        }
    }
}