using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "MsSqlScriptIndexGenerator", Description = "Generate Sql script index for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlScriptIndexGeneratorMsContextAction : SqlScriptIndexGeneratorContextActionBase
    {
        public SqlScriptIndexGeneratorMsContextAction(ICSharpContextActionDataProvider provider) : base(provider, GenerateSqlIndex)
        {
        }

        [NotNull]
        public static string GenerateSqlIndex((string TableName, string ColumnName) arg)
        {
            var (tableName, columnName) = arg;
            var indexName = $"IX_{tableName}_{columnName}";

            return $"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE NAME ='{indexName}' AND object_id = OBJECT_ID('{tableName}'))\r\n" +
                   $"   CREATE INDEX[{indexName}] ON[{tableName}]([{columnName}])\r\n" +
                   "   with(online = on)\r\n" +
                   "GO";
        }


        public override string Text => "Generate ms sql index script";
    }
}