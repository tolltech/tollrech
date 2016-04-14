using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.reflection2.elements.Compiled;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech
{
    [ContextAction(Group = "C#", Name = "Unit Test Action", Description = "Fill unit tests' mocks")]
    public class UnitTestFiller : ContextActionBase
    {
        private ICSharpContextActionDataProvider Provider { get; set; }

        public UnitTestFiller(ICSharpContextActionDataProvider provider)
        {
            Provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Work();
            return null;
        }

        public override string Text { get; } = "Fill unit tests' mocks";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            var method = Provider.GetSelectedElement<IMethodDeclaration>();

            var insideOfMethod = method != null;

            if (insideOfMethod)
            {
                return true;
            }

            return false;
        }

        private void Work()
        {
            var expression = Provider.GetSelectedElement<IInvocationExpression>();
            var declaredElement = expression.Reference.CurrentResolveResult.DeclaredElement;
            var d = (Method) declaredElement;

            //expresssion.
            //var type1 = expresssion.TypeArguments;

            //IMethodDeclaration method = Provider.GetSelectedElement<IMethodDeclaration>();

            //IType type = method.DeclaredElement.ReturnType;

            //string typePresentableName = type.GetPresentableName(CSharpLanguage.Instance);

            //CSharpElementFactory factory = CSharpElementFactory.GetInstance(Provider.PsiModule);

            //string code = $"new {typePresentableName}()";

            //ICSharpExpression newExpression = factory.CreateExpression(code);

            //IReturnStatement returnStatement = Provider.GetSelectedElement<IReturnStatement>(false);

            //returnStatement.SetValue(newExpression);
        }
    }
}