using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "PSqlScriptIndexGenerator", Description = "Generate Sql script index for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlScriptIndexGeneratorPostgreContextAction : SqlScriptIndexGeneratorContextActionBase
    {
        public SqlScriptIndexGeneratorPostgreContextAction(ICSharpContextActionDataProvider provider) : base(provider, GenerateSqlIndex)
        {
        }

        [NotNull]
        private static string GenerateSqlIndex((string TableName, string ColumnName) arg)
        {
            var (tableName, columnName) = arg;
            return $"CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_{tableName}_{columnName} ON {tableName} ({columnName});";
        }


        public override string Text => "Generate psql index script";
    }
}