using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
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
                if (propertyDeclaration.Attributes.Any(x => x.Name.NameIdentifier.Name == Constants.JsonProperty))
                {
                    continue;
                }

                if (!propertyDeclaration.HasGetSet())
                {
                    continue;
                }

                var attribute = CreateAttribute();

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

        private static readonly ConcurrentDictionary<string, IDeclaredType> cachedAttributes = new ConcurrentDictionary<string, IDeclaredType>();
        private IDeclaredType GetCachedType()
        {
            return cachedAttributes.GetOrAdd(Constants.JsonProperty, x => TypeFactory.CreateTypeByCLRName(new ClrTypeName($"Newtonsoft.Json.{Constants.JsonProperty}Attribute"), provider.PsiModule));
        }

        [CanBeNull]
        private IAttribute CreateAttribute()
        {
            var attributeType = GetCachedType();
            var attributeTypeElement = attributeType.GetTypeElement();

            if (attributeTypeElement == null)
            {
                return null;
            }

            return factory.CreateAttribute(attributeTypeElement);
        }

        public override bool IsAvailable(IUserDataHolder cache) => classDeclaration != null && classDeclaration.PropertyDeclarations.Any(x => x.HasGetSet());
    }
}