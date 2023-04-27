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

        //ALTER TABLE [TableName] ADD COLUMN IF NOT EXISTS [ColumnName] nvarchar(10) NULL;

        //-- вариант 1 - создание с последующим удалением значения по умолчанию
        //    alter table table_name add column if not exists column_name bigint not null default (0);
        //alter table table_name alter column column_name drop default;

        [NotNull]
        private static string GenerateCustomPropertyScript((string TableName, PropertyInfo PropertyInfo) arg)
        {
            var (tableName, propertyInfo) = arg;
            var sb = new StringBuilder();
            sb.Append($"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS");
            AddPropertyTypeInfo(sb, propertyInfo);

            sb.Append(!propertyInfo.Required ? " NULL" : $" NOT NULL DEFAULT(0)");

            sb.AppendLine(";");
            sb.AppendLine();

            if (propertyInfo.Required)
            {
                sb.AppendLine("-- Put it in covertDb.sql, sir");
                sb.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {propertyInfo.ColumnName} DROP DEFAULT;");
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

            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName}(");

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                sb.Append($"   ");

                AddPropertyTypeInfo(sb, property);
                AddRequiredInfo(sb, property);

                if (i != properties.Length - 1)
                {
                    sb.Append(',');
                }

                sb.AppendLine();
            }

            sb.AppendLine(")");

            return sb.ToString();
        }

        private static void AddPropertyTypeInfo([NotNull] StringBuilder sb, PropertyInfo property)
        {
            var columnType = property.GetColumnType(DbType.Postgres);
            sb.Append($" {property.ColumnName} {columnType}");
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

        public override string Text => "Generate psql script";

        //ALTER TABLE [TableName] DROP COLUMN IF EXISTS [ColumnName];
    }
}