using System;
using System.Collections.Generic;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlMapGenerate", Description = "Generate Sql map for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlMapGeneratorContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;
        private readonly IClassDeclaration classDeclaration;
        private readonly CSharpElementFactory factory;

        public SqlMapGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            this.provider = provider;
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            if (classDeclaration.Attributes.FindAttribute(Constants.Table) == null)
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

                var columnAttribute = CreateSchemaAttribute(Constants.Column);

                if (columnAttribute == null)
                {
                    return;
                }

                var columnName = propertyDeclaration.NameIdentifier.Name;

                var columnNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{columnName}"));
                columnAttribute.AddArgumentBefore(columnNameArgument, null);

                var propertyType = propertyDeclaration.Type;
                var typeNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateExpression($"{Constants.TypeName} = $0", GetMappingTypeName(propertyType)));
                columnAttribute.AddArgumentBefore(typeNameArgument, null);

                propertyDeclaration.AddAttributeAfter(columnAttribute, propertyDeclaration.Attributes.LastOrDefault());

                AddAnnotationAttributesIfNeed(propertyDeclaration);
            }
        }

        private void AddAnnotationAttributesIfNeed(IPropertyDeclaration propertyDeclaration)
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

        private ICSharpExpression GetMappingTypeName(IType scalarType)
        {
            var columnTypeNameClass = provider.GetType($"SKBKontur.Billy.Core.Common.Quering.ColumnTypeNames");
            var columnTypeNameClassType = columnTypeNameClass.GetTypeElement();

            // ReSharper disable once InconsistentNaming
            ICSharpExpression createExpression(string x) => columnTypeNameClassType != null
                ? factory.CreateReferenceExpression("$0.$1", columnTypeNameClassType.ShortName, x)
                : factory.CreateExpression($"ColumnTypeNames.{x}");

            if (scalarType.IsNullable())
            {
                scalarType = scalarType.GetNullableUnderlyingType();
            }

            if (scalarType.IsInt() || scalarType.IsEnumType())
            {
                return createExpression(Constants.Int);
            }

            if (scalarType.IsGuid())
            {
                return createExpression(Constants.UniqueIdentifier);
            }

            if (scalarType.IsString())
            {
                return createExpression(Constants.NVarChar);
            }

            if (scalarType.IsBool())
            {
                return createExpression(Constants.Bit);
            }

            if (scalarType.IsDateTime())
            {
                return createExpression(Constants.DateTime2);
            }

            if (scalarType.IsLong())
            {
                return createExpression(Constants.BigInt);
            }

            if (scalarType.IsDecimal())
            {
                return createExpression(Constants.Decimal);
            }

            return factory.CreateExpression("TODO");
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
            var tableAttribute = CreateSchemaAttribute(Constants.Table);

            if (tableAttribute == null)
            {
                return;
            }

            var tableName = classDeclaration.NameIdentifier.Name.MorphemToManies();
            var tableAttributeArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{tableName}"));
            tableAttribute.AddArgumentAfter(tableAttributeArgument, null);

            classDeclaration.AddAttributeBefore(tableAttribute, null);
        }

        private IAttribute CreateSchemaAttribute(string attributeShortTypeName) => provider.CreateAttribute($"System.ComponentModel.DataAnnotations.Schema.{attributeShortTypeName}Attribute");

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