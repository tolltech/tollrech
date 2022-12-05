using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "SqlScriptIndexByMethodGeneratorPostgreContextAction", Description = "Generate Sql script index for handler methods", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlScriptIndexByMethodGeneratorPostgreContextAction : SqlScriptIndexByMethodGeneratorContextActionBase
    {
        public SqlScriptIndexByMethodGeneratorPostgreContextAction(ICSharpContextActionDataProvider provider) : base(provider, GetIndexScript)
        {
        }

        public static string GetIndexScript(string tableName, string[] propertyNames)
        {
            return $"CREATE INDEX CONCURRENTLY IF NOT exists ix_{tableName}_{string.Join("_", propertyNames)} ON {tableName} ({string.Join(", ", propertyNames)});";
        }

        public override string Text => "Generate psql index script";
    }
}