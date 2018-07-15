using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlScriptGenerator", Description = "Generate Sql script for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlScriptGeneratorContextAction : ContextActionBase
    {
        private readonly IClassDeclaration classDeclaration;
        private readonly IPropertyDeclaration propertyDeclaration;
        private readonly CSharpElementFactory factory;
        private IAttribute propertyColumnAttribute;
        private IAttribute tableAttribute;

        public SqlScriptGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>(); ;
            propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            (string Script, ITypeMemberDeclaration Declaration) result;
            if (propertyColumnAttribute != null)
            {
                result.Script = GeneratePropertyScript();
                result.Declaration = propertyDeclaration;
            }
            else if (tableAttribute != null)
            {
                result.Script = GenerateTableScript(tableAttribute);
                result.Declaration = classDeclaration;
            }
            else
            {
                return null;
            }

            AddXmlComment(result.Declaration, result.Script);
            return null;
        }

        private void AddXmlComment(ITypeMemberDeclaration declaration, string text)
        {
            var docCommentBlockOwnerNode = XmlDocTemplateUtil.FindDocCommentOwner(declaration);

            if (docCommentBlockOwnerNode == null)
            {
                return;
            }

            var comment = factory.CreateDocCommentBlock(text);
            docCommentBlockOwnerNode.SetDocCommentBlock(comment);
        }

        [NotNull]
        private string GeneratePropertyScript()
        {
            var propertyInfo = GetPropertyInfo(propertyDeclaration);
            var tableName = tableAttribute.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";

            var sb = new StringBuilder();
            sb.AppendLine($"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{propertyInfo.ColumnName}')");
            sb.Append($"    ALTER TABLE [{tableName}] ADD");
            AddPropertyTypeInfo(sb, propertyInfo);

            sb.Append(!propertyInfo.Required ? " NULL" : $" CONSTRAINT DF_{tableName}_{propertyInfo.ColumnName} default (0) NOT NULL");

            sb.AppendLine(";");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (propertyInfo.Required)
            {
                sb.AppendLine("-- Put it in covertDb.sql, sir");
                sb.AppendLine($"IF EXISTS(SELECT* FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{propertyInfo.ColumnName}' and COLUMN_DEFAULT IS NOT NULL)");
                sb.AppendLine($"    ALTER TABLE [{tableName}] DROP CONSTRAINT [DF_{tableName}_{propertyInfo.ColumnName}]");
                sb.AppendLine("GO");
            }

            return sb.ToString();
        }

        [NotNull]
        private string GenerateTableScript([NotNull] IAttribute attribute)
        {
            var tableName = attribute.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";
            var properties = classDeclaration.PropertyDeclarations
                .Where(x => x.HasGetSet())
                .Select(GetPropertyInfo)
                .ToArray();

            var sb = new StringBuilder();

            sb.AppendLine($"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}')");
            sb.AppendLine($"CREATE TABLE [dbo].[{tableName}](");

            foreach (var property in properties)
            {
                sb.Append($"   ");


                AddPropertyTypeInfo(sb, property);
                AddRequiredInfo(sb, property);

                sb.Append(',');
                sb.AppendLine();
            }

            if (properties.Any(x => x.Key))
            {
                var keyProperty = properties.First(x => x.Key);
                sb.AppendLine($"    CONSTRAINT[PK_{tableName}] PRIMARY KEY CLUSTERED([{keyProperty.ColumnName}] ASC)");
            }

            sb.AppendLine(")");

            return sb.ToString();
        }

        private static (string ColumnName, string ColumnType, bool Required, bool Key, string MaxLength, string Precision1, string Precision2)
            GetPropertyInfo([NotNull] IPropertyDeclaration propertyDeclaration)
        {
            return (
                ColumnName: propertyDeclaration.Attributes.FindAttribute(Constants.Column)?.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOColumnName",
                ColumnType: GetColumnType(propertyDeclaration),
                Required: propertyDeclaration.Attributes.HasAttriobute(Constants.Required),
                Key: propertyDeclaration.Attributes.HasAttriobute(Constants.Key),
                MaxLength: propertyDeclaration.Attributes.FindAttribute(Constants.MaxLength)?.Arguments.FirstOrDefault().GetLiteralText(),
                Precision1: propertyDeclaration.Attributes.FindAttribute(Constants.DecimalPrecision)?.Arguments.FirstOrDefault().GetLiteralText(),
                Precision2: propertyDeclaration.Attributes.FindAttribute(Constants.DecimalPrecision)?.Arguments.LastOrDefault().GetLiteralText()
            );
        }

        private void AddPropertyTypeInfo([NotNull] StringBuilder sb, (string ColumnName, string ColumnType, bool Required, bool Key, string MaxLength, string Precision1, string Precision2) property)
        {
            sb.Append($" [{property.ColumnName}] [{property.ColumnType}]");
            if (!string.IsNullOrWhiteSpace(property.MaxLength))
            {
                sb.Append($" ({property.MaxLength})");
            }

            if (!string.IsNullOrWhiteSpace(property.Precision1) && !string.IsNullOrWhiteSpace(property.Precision2))
            {
                sb.Append($" ({property.Precision1}, {property.Precision2})");
            }
        }

        private void AddRequiredInfo(StringBuilder sb, (string ColumnName, string ColumnType, bool Required, bool Key, string MaxLength, string Precision1, string Precision2) property)
        {
            if (property.Required)
            {
                sb.Append(" NOT");
            }

            sb.Append(" NULL");
        }

        [NotNull]
        private static string GetColumnType([NotNull] IPropertyDeclaration pD)
        {
            var typeNameExpression = pD.Attributes.FindAttribute(Constants.Column)?.PropertyAssignments.FirstOrDefault(x => x.PropertyNameIdentifier.Name == Constants.TypeName)?.Source;
            if (typeNameExpression is ICSharpLiteralExpression literalExpression)
            {
                return literalExpression.GetText().Trim('"');
            }

            if (typeNameExpression is IReferenceExpression referenceExpression)
            {
                var codedType = referenceExpression.NameIdentifier.Name;
                return codedTypes.TryGetValue(codedType, out var value) ? value : codedType.ToLower();
            }

            return "TODOColumnType";
        }

        private static readonly Dictionary<string, string> codedTypes = new Dictionary<string, string>
                                                                {
                                                                    {Constants.BigInt, "bigint" },
                                                                    {Constants.Bit, "bit" },
                                                                    {Constants.DateTime2, "datetime2" },
                                                                    {Constants.Decimal, "decimal" },
                                                                    {Constants.Int, "int" },
                                                                    {Constants.UniqueIdentifier, "uniqueidentifier" },
                                                                    {Constants.NVarChar, "nvarchar" },
                                                                    {Constants.Date, "date" }
                                                                };

        public override string Text => "Generate sql-script";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            propertyColumnAttribute = propertyDeclaration?.Attributes.FindAttribute(Constants.Column);
            tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table);
            return tableAttribute != null;
        }

        //IF EXISTS(SELECT* FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TableName' AND COLUMN_NAME = 'ColumnName')
        //    ALTER TABLE[TableName] DROP COLUMN[ColumnName];
        //GO

        //IF EXISTS(SELECT 1 FROM sys.columns where object_id = OBJECT_ID('TableName') and name = 'ColumnName' and is_nullable = 0)
        //BEGIN
        //    ALTER TABLE[TableName] ALTER COLUMN[ColumnName][sameType] NULL;
        //END
        //GO
    }
}