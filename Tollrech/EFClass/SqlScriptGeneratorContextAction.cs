using System;
using System.Linq;
using System.Text;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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

            result.Declaration.AddXmlComment(result.Script, factory);
            return null;
        }

        private string GeneratePropertyScript()
        {
            var propertyInfo = propertyDeclaration.GetPropertyInfo();
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

        private string GenerateTableScript(IAttribute attribute)
        {
            var tableName = attribute.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";
            var properties = classDeclaration.PropertyDeclarations
                .Where(x => x.HasGetSet())
                .Where(x => x.Attributes.FindAttribute(Constants.Column) != null)
                .Select(x => x.GetPropertyInfo())
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

        private void AddPropertyTypeInfo(StringBuilder sb, PropertyInfo property)
        {
            sb.Append($" [{property.ColumnName}] [{property.GetColumnType()}]");
            if (property.GetColumnType() == "nvarchar")
            {
                if (!string.IsNullOrWhiteSpace(property.MaxLength))
                {
                    sb.Append($" ({property.MaxLength})");
                }
                else
                {
                    sb.Append(" (MAX)");
                }
            }

            if (!string.IsNullOrWhiteSpace(property.Precision1) && !string.IsNullOrWhiteSpace(property.Precision2))
            {
                sb.Append($" ({property.Precision1}, {property.Precision2})");
            }
        }

        private void AddRequiredInfo(StringBuilder sb, PropertyInfo property)
        {
            if (property.Required || property.IsTimestamp)
            {
                sb.Append(" NOT");
            }

            sb.Append(" NULL");
        }

        public override string Text => "Generate sql script";

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