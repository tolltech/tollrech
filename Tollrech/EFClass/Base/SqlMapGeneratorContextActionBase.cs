using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass.Base
{
    public abstract class SqlMapGeneratorContextActionBase : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;
        private readonly string columnTypeNameClassName;
        private readonly Func<IType, string> GetDbColumnTypeName;
        private readonly Func<string, string> convertCaseStyle;
        private readonly IClassDeclaration classDeclaration;
        private readonly CSharpElementFactory factory;

        [NotNull]
        protected virtual string tableAttributeName => Constants.Table;
        [NotNull]
        protected virtual string tableAttributeNamespace => DataAnnotationsNamespace;

        private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations.Schema";

        protected SqlMapGeneratorContextActionBase(ICSharpContextActionDataProvider provider, [NotNull] string columnTypeNameClassName, [NotNull] Func<IType, string> getDbColumnTypeName,
                                                   [CanBeNull] Func<string, string> convertColumnName = null)
        {
            this.provider = provider;
            this.columnTypeNameClassName = columnTypeNameClassName;
            GetDbColumnTypeName = getDbColumnTypeName;
            this.convertCaseStyle = convertColumnName ?? (x => x);
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            if (classDeclaration.Attributes.FindAttribute(tableAttributeName) == null)
            {
                AddTableAttribute();
            }

            AddColumnAttributes();

            return null;
        }

        private void AddColumnAttributes()
        {
            foreach (var propertyDeclaration in classDeclaration.PropertyDeclarations)
            {
                if (propertyDeclaration.HasAttribute(Constants.Column))
                {
                    continue;
                }

                if (!propertyDeclaration.HasGetSet())
                {
                    continue;
                }

                var columnAttribute = CreateSchemaAttribute(Constants.Column, DataAnnotationsNamespace);

                if (columnAttribute == null)
                {
                    return;
                }

                var columnName = convertCaseStyle(propertyDeclaration.NameIdentifier.Name);

                var columnNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{columnName}"));
                columnAttribute.AddArgumentBefore(columnNameArgument, null);

                var propertyType = propertyDeclaration.Type;
                var typeNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateExpression($"{Constants.TypeName} = $0", GetMappingTypeName(propertyType)));
                columnAttribute.AddArgumentBefore(typeNameArgument, null);

                propertyDeclaration.AddAttributeAfter(columnAttribute, propertyDeclaration.Attributes.LastOrDefault());

                AddAnnotationAttributesIfNeed(propertyDeclaration);
            }
        }

        private void AddAnnotationAttributesIfNeed([NotNull] IPropertyDeclaration propertyDeclaration)
        {
            var propertyType = propertyDeclaration.Type;
            if (propertyDeclaration.NameIdentifier.Name == Constants.Id)
            {
                AddAnnotationAttribute(propertyDeclaration, Constants.Key);
            }

            AddAnnotationAttribute(propertyDeclaration, Constants.ConcurrencyCheck);

            if (!propertyType.IsNullable())
            {
                List<ICSharpExpression> requiredAttributeArguments = null;

                if (propertyType.IsString())
                {
                    requiredAttributeArguments = new List<ICSharpExpression> { factory.CreateExpression($"{Constants.AllowEmptyStrings} = true") };
                }

                AddAnnotationAttribute(propertyDeclaration, Constants.Required, requiredAttributeArguments?.ToArray() ?? Array.Empty<ICSharpExpression>());
            }

            if (propertyType.IsDecimal())
            {
                var precisionArguments = new[]
                                         {
                                             factory.CreateExpression("18"),
                                             factory.CreateExpression("2"),
                                         };

                AddAnnotationAttribute(propertyDeclaration, $"SKBKontur.Billy.Core.Database.Sql.Attributes.{Constants.DecimalPrecision}", precisionArguments);
            }

            if (propertyType.IsString())
            {
                AddAnnotationAttribute(propertyDeclaration, Constants.MaxLength, factory.CreateExpression("TODO"));
            }
        }

        [NotNull]
        private ICSharpExpression GetMappingTypeName(IType scalarType)
        {
            var columnTypeNameClass = provider.GetType($"SKBKontur.Billy.Core.Database.Sql.{columnTypeNameClassName}");
            var columnTypeNameClassType = columnTypeNameClass.GetTypeElement();

            // ReSharper disable once InconsistentNaming
            ICSharpExpression createExpression(string x) => columnTypeNameClassType != null
                ? factory.CreateReferenceExpression("$0.$1", columnTypeNameClassType.ShortName, x)
                : factory.CreateExpression($"{columnTypeNameClassName}.{x}");

            if (scalarType.IsNullable())
            {
                scalarType = scalarType.GetNullableUnderlyingType();
            }

            var dbColumnTypeName = GetDbColumnTypeName(scalarType);
            return string.IsNullOrWhiteSpace(dbColumnTypeName) ? factory.CreateExpression("TODO") : createExpression(dbColumnTypeName);
        }

        private void AddAnnotationAttribute(IPropertyDeclaration propertyDeclaration, string attributeName, params ICSharpExpression[] argumentsExpressions)
        {
            var annotationAttribute = CreateAnnotationAttribute(attributeName);
            if (annotationAttribute == null)
            {
                return;
            }

            if (argumentsExpressions.Length > 0)
            {
                foreach (var argumentsExpression in argumentsExpressions)
                {
                    annotationAttribute.AddArgumentBefore(factory.CreateArgument(ParameterKind.VALUE, argumentsExpression), null);
                }
            }

            propertyDeclaration.AddAttributeAfter(annotationAttribute, propertyDeclaration.Attributes.LastOrDefault());
        }

        private void AddTableAttribute()
        {
            var tableAttribute = CreateSchemaAttribute(tableAttributeName, tableAttributeNamespace);

            if (tableAttribute == null)
            {
                return;
            }

            var tableName = convertCaseStyle(classDeclaration.NameIdentifier.Name.MorphemToManies());
            var tableAttributeArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{tableName}"));
            tableAttribute.AddArgumentAfter(tableAttributeArgument, null);

            classDeclaration.AddAttributeBefore(tableAttribute, null);
        }

        private IAttribute CreateSchemaAttribute(string attributeShortTypeName, string attributeNamespace) => provider.CreateAttribute($"{attributeNamespace}.{attributeShortTypeName}Attribute");

        [CanBeNull]
        private IAttribute CreateAnnotationAttribute(string attributeTypeName)
	        => provider.CreateAttribute($"System.ComponentModel.DataAnnotations.{attributeTypeName}Attribute")
		        ?? provider.CreateAttribute($"{attributeTypeName}Attribute");

        public override string Text => "Add data annotation mapping";

        public override bool IsAvailable(IUserDataHolder cache)
        {
	        return classDeclaration.HasAnyGetSetProperty();
        }
    }
}