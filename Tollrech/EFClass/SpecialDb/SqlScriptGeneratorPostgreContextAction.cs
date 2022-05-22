using System.Text;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "PSqlScriptGenerator", Description = "Generate PSql script for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlScriptGeneratorPostgreContextAction : SqlScriptGeneratorContextActionBase
    {
        public SqlScriptGeneratorPostgreContextAction(ICSharpContextActionDataProvider provider) : base(provider, GenerateCustomPropertyScript, GenerateCustomTableScript)
        {
        }

        [NotNull]
        private static string GenerateCustomPropertyScript((string TableName, PropertyInfo PropertyInfo) arg)
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


        //CREATE TABLE IF NOT EXISTS [TableName](
        //    id uuid PRIMARY KEY NOT NULL,
        //    state int4 NOT NULL,
        //    number varchar(50) NOT NULL,
        //    ticks int8 NOT NULL
        //);

        [NotNull]
        private static string GenerateCustomTableScript((string TableName, PropertyInfo[] Properties) arg)
        {
            var (tableName, properties) = arg;
            var sb = new StringBuilder();

            sb.AppendLine($"CREATE TABLE IF NOT EXISTS [{tableName}](");

            foreach (var property in properties)
            {
                sb.Append($"   ");

                AddPropertyTypeInfo(sb, property);
                AddRequiredInfo(sb, property);

                sb.Append(',');
                sb.AppendLine();
            }

            sb.AppendLine(")");

            return sb.ToString();
        }

        private static void AddPropertyTypeInfo([NotNull] StringBuilder sb, PropertyInfo property)
        {
            var columnType = property.GetColumnType(DbType.Postgres);
            sb.Append($" [{property.ColumnName}] [{columnType}]");
            if (columnType == "varchar")
            {
                if (!string.IsNullOrWhiteSpace(property.MaxLength))
                {
                    sb.Append($" ({property.MaxLength})");
                }
            }

            if (!string.IsNullOrWhiteSpace(property.Precision1) && !string.IsNullOrWhiteSpace(property.Precision2))
            {
                sb.Append($" ({property.Precision1}, {property.Precision2})");
            }

            if (property.Key)
            {
                sb.Append($" PRIMARY KEY");
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

        public override string Text => "(doesn't work) Generate psql script";


        //ALTER TABLE [TableName] DROP COLUMN IF EXISTS [ColumnName];

        //ALTER TABLE [TableName] ADD COLUMN IF NOT EXISTS [ColumnName] nvarchar(10) NULL;
    }
}