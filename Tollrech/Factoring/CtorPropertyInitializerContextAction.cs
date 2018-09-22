using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
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
        private readonly CSharpElementFactory factory;

        public CtorPropertyInitializerContextAction(ICSharpContextActionDataProvider provider)
        {
            this.provider = provider;
            factory = provider.ElementFactory;
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

            var objectInitializer = ctorExpression.Initializer as IObjectInitializer ?? factory.CreateObjectInitializer();

            var initializedProperties = new HashSet<string>(objectInitializer.MemberInitializers.OfType<IPropertyInitializer>().Select(x => x.MemberName));

            var classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            var hasTestBaseSuperType = classDeclaration.GetAllSuperTypes().Any(x => x.GetClassType()?.ShortName.Contains("TestBase") ?? false);

            var dummyHelper = new DummyHelper();
            var properiesToInitialize = new List<(string Name, string Value)>();
            foreach (var property in properties.Where(x => !initializedProperties.Contains(x.ShortName)))
            {
                var propertyDummyValue = hasTestBaseSuperType
                    ? dummyHelper.GetParamValue(property.Type, property.ShortName)
                    : "TODO";
                properiesToInitialize.Add((Name: property.ShortName, Value: propertyDummyValue));
            }

            foreach (var propertyInitializer in properiesToInitialize)
            {
                var initializer = factory.CreateObjectPropertyInitializer(propertyInitializer.Name, factory.CreateExpression("$0", propertyInitializer.Value));
                objectInitializer.AddMemberInitializerBefore(initializer, null);
            }

            ctorExpression.SetInitializer(objectInitializer);

            return null;
        }

        public override string Text => "Create property initializers";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return ctorExpression != null;
        }
    }
}