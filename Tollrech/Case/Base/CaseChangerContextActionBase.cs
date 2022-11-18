using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech.Case.Base
{
    public abstract class CaseContextActionBase : ContextActionBase
    {
        private readonly Func<string, string> caseTransform;

        private readonly IClassDeclaration classDeclaration;
        private readonly ICSharpLiteralExpression literalExpression;
        private readonly CSharpElementFactory factory;

        protected CaseContextActionBase([NotNull] ICSharpContextActionDataProvider provider, [CanBeNull] Func<string, string> propertyNameTransform = null)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            literalExpression = provider.GetSelectedElement<ICSharpLiteralExpression>();
            this.caseTransform = propertyNameTransform ?? (x=> x);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var text = literalExpression.Literal.GetText().Trim('"');
            literalExpression.ReplaceBy(factory.CreateStringLiteralExpression(caseTransform(text)));

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache) => classDeclaration != null && literalExpression != null;
    }
}