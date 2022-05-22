using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "MsSqlScriptGenerator", Description = "Generate MsSql script for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlScriptGeneratorMsContextAction : SqlScriptGeneratorContextActionBase
    {
        public SqlScriptGeneratorMsContextAction(ICSharpContextActionDataProvider provider) : base(provider, GenerateCustomPropertyScript, GenerateCustomTableScript)
        {
        }

        [NotNull]
        public static string GenerateCustomPropertyScript((string TableName, PropertyInfo PropertyInfo) arg)
        {
            var (tableName, propertyInfo) = arg;
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
        public static string GenerateCustomTableScript((string TableName, PropertyInfo[] Properties) arg)
        {
            var (tableName, properties) = arg;
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

        private static void AddPropertyTypeInfo([NotNull] StringBuilder sb, PropertyInfo property)
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

        private static void AddRequiredInfo([NotNull] StringBuilder sb, PropertyInfo property)
        {
            if (property.Required || property.IsTimestamp)
            {
                sb.Append(" NOT");
            }

            sb.Append(" NULL");
        }

        public override string Text => "Generate mssql script";

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