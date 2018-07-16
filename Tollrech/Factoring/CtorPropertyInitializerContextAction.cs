using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.Factoring
{
    [ContextAction(Name = "CtorPropertyInitializer", Description = "Create property initializer", Group = "C#", Disabled = false, Priority = 1)]
    public class CtorPropertyInitializerContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;
        private readonly IObjectCreationExpression ctorExpression;

        public CtorPropertyInitializerContextAction(ICSharpContextActionDataProvider provider)
        {
            this.provider = provider;
            ctorExpression = provider.GetSelectedElement<IObjectCreationExpression>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var type = ctorExpression.Type();

            if (!type.IsResolved)
            {
                return null;
            }

            var properties = type.GetClassType()?.Properties;
            if (properties == null)
            {
                return null;
            }

            var dummyHelper = new DummyHelper();
            foreach (var property in properties)
            {
                var propertyDummyValue = dummyHelper.GetParamValue(property.Type, property.ShortName);
            }

            return null;
        }

        public override string Text => "Create property initializaers";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return ctorExpression != null;
        }
    }
}