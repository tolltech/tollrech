using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech.Logging
{
    [ContextAction(Name = "LogIntroducer", Description = "Introduce logger", Group = "C#", Disabled = false, Priority = 1)]
    public class LogIntroducerContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;
        private readonly CSharpElementFactory factory;
        private readonly IClassDeclaration classDeclaration;
        private readonly ICSharpLiteralExpression literalExpression;

        public LogIntroducerContextAction(ICSharpContextActionDataProvider provider)
        {
            this.provider = provider;
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            literalExpression = provider.GetSelectedElement<ICSharpLiteralExpression>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            return null;
        }

        public override string Text => "Log this";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return classDeclaration != null  && literalExpression != null;
        }
    }
}
