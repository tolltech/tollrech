using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

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
            if (!classDeclaration.Attributes.Any(x => x.Name.NameIdentifier.Name == "Table"))
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
                if (propertyDeclaration.Attributes.Any(x => x.Name.NameIdentifier.Name == "Column"))
                {
                    continue;
                }

                var columnAttribute = CreateSchemaAttribute("Column");

                if (columnAttribute == null)
                {
                    return;
                }

                var columnName = propertyDeclaration.NameIdentifier.Name;

                var columnNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{columnName}"));
                columnAttribute.AddArgumentBefore(columnNameArgument, null);

                var propertyType = propertyDeclaration.Type;
                var typeNameArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateExpression("TypeName = $0", GetMappingTypeName(propertyType)));
                columnAttribute.AddArgumentBefore(typeNameArgument, null);

                propertyDeclaration.AddAttributeAfter(columnAttribute, propertyDeclaration.Attributes.LastOrDefault());

                AddAnnotationAttributesIfNeed(propertyDeclaration);
            }
        }

        private void AddAnnotationAttributesIfNeed([NotNull] IPropertyDeclaration propertyDeclaration)
        {
            var propertyType = propertyDeclaration.Type;
            if (propertyDeclaration.NameIdentifier.Name == "Id")
            {
                AddAnnotationAttribute(propertyDeclaration, "Key");
            }

            AddAnnotationAttribute(propertyDeclaration, "ConcurrencyCheck");

            if (!propertyType.IsNullable())
            {
                List<ICSharpExpression> requiredAttributeArguments = null;

                if (propertyType.IsString())
                {
                    requiredAttributeArguments = new List<ICSharpExpression> { factory.CreateExpression("AllowEmptyStrings = true") };
                }

                AddAnnotationAttribute(propertyDeclaration, "Required", requiredAttributeArguments?.ToArray() ?? Array.Empty<ICSharpExpression>());
            }

            if (propertyType.IsDecimal())
            {
                var precisionArguments = new[]
                                         {
                                             factory.CreateExpression("$0", factory.CreateStringLiteralExpression("18")),
                                             factory.CreateExpression("$0", factory.CreateStringLiteralExpression("2")),
                                         };

                AddAnnotationAttribute(propertyDeclaration, "DecimalPrecision", precisionArguments);
            }
        }

        [NotNull]
        private ICSharpExpression GetMappingTypeName(IType scalarType)
        {
            if (scalarType.IsNullable())
            {
                scalarType = scalarType.GetNullableUnderlyingType();
            }

            if (scalarType.IsInt() || scalarType.IsEnumType())
            {
                return factory.CreateExpression("ColumnTypeNames.Int");
            }

            if (scalarType.IsGuid())
            {
                return factory.CreateExpression("ColumnTypeNames.UniqueIdentifier");
            }

            if (scalarType.IsString())
            {
                return factory.CreateExpression("ColumnTypeNames.NVarChar");
            }

            if (scalarType.IsBool())
            {
                return factory.CreateExpression("ColumnTypeNames.Bit");
            }

            if (scalarType.IsDateTime())
            {
                return factory.CreateExpression("ColumnTypeNames.DateTime2");
            }

            if (scalarType.IsLong())
            {
                return factory.CreateExpression("ColumnTypeNames.BigInt");
            }

            if (scalarType.IsDecimal())
            {
                return factory.CreateExpression("ColumnTypeNames.Decimal");
            }

            return factory.CreateStringLiteralExpression("TODO");
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
            var tableAttribute = CreateSchemaAttribute("Table");

            if (tableAttribute == null)
            {
                return;
            }

            var tableName = MorphemToManies(classDeclaration.NameIdentifier.Name);
            var tableAttributeArgument = factory.CreateArgument(ParameterKind.VALUE, factory.CreateStringLiteralExpression($"{tableName}"));
            tableAttribute.AddArgumentAfter(tableAttributeArgument, null);

            classDeclaration.AddAttributeBefore(tableAttribute, null);
        }

        [CanBeNull]
        private IAttribute CreateSchemaAttribute(string attributeShortTypeName)
        {
            var attributeType = TypeFactory.CreateTypeByCLRName(new ClrTypeName($"System.ComponentModel.DataAnnotations.Schema.{attributeShortTypeName}Attribute"), provider.PsiModule);
            var attributeTypeElement = attributeType.GetTypeElement();

            if (attributeTypeElement == null)
            {
                return null;
            }

            return factory.CreateAttribute(attributeTypeElement);
        }

        [CanBeNull]
        private IAttribute CreateAnnotationAttribute(string attributeShortTypeName)
        {
            var attributeType = TypeFactory.CreateTypeByCLRName(new ClrTypeName($"System.ComponentModel.DataAnnotations.{attributeShortTypeName}Attribute"), provider.PsiModule);
            var attributeTypeElement = attributeType.GetTypeElement();

            if (attributeTypeElement == null)
            {
                return null;
            }

            return factory.CreateAttribute(attributeTypeElement);
        }

        [NotNull]
        private static string MorphemToManies(string src)
        {
            if (src.EndsWith("Dbo", StringComparison.InvariantCultureIgnoreCase))
            {
                src = src.TrimFromEnd("Dbo", StringComparison.InvariantCultureIgnoreCase);
            }

            if (src.EndsWith("s") || src.EndsWith("o"))
            {
                return $"{src}es";
            }

            if (src.EndsWith("y"))
            {
                return $"{src.TrimEnd('y')}ies";
            }

            return $"{src}s";
        }

        public override string Text => "Add data annotation mapping";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return classDeclaration != null;
        }
    }
}