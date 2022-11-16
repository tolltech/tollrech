using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.Json.Base
{
    public abstract class JsonPropertyContextActionBase : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;

        private readonly Func<string, string> propertyNameTransform;

        private readonly IClassDeclaration classDeclaration;
        private readonly CSharpElementFactory factory;

        protected JsonPropertyContextActionBase([NotNull] ICSharpContextActionDataProvider provider, [CanBeNull] Func<string, string> propertyNameTransform = null)
        {
            this.provider = provider;
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            this.propertyNameTransform = propertyNameTransform ?? (x=> x);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            AddJsonPropertyAttributes();

            return null;
        }

        private void AddJsonPropertyAttributes()
        {
            foreach (var propertyDeclaration in classDeclaration.PropertyDeclarations)
            {
	            if (propertyDeclaration.HasAttribute(Constants.JsonProperty))
                {
                    continue;
                }

                if (!propertyDeclaration.HasGetSet())
                {
                    continue;
                }

                var attribute = provider.CreateAttribute($"Newtonsoft.Json.{Constants.JsonProperty}Attribute");

                if (attribute == null)
                {
                    return;
                }

                var propertyName = propertyDeclaration.NameIdentifier.Name;

                var propertyNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{propertyNameTransform(propertyName)}"));
                attribute.AddArgumentBefore(propertyNameArgument, null);
                propertyDeclaration.AddAttributeAfter(attribute, propertyDeclaration.Attributes.LastOrDefault());
            }
        }

        public override bool IsAvailable(IUserDataHolder cache) => classDeclaration.HasAnyGetSetProperty();
    }
}