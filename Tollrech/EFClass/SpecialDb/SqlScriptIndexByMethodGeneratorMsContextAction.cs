using System.Linq;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "SqlScriptIndexByMethodGeneratorMsContextAction", Description = "Generate Sql script index for handler methods", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlScriptIndexByMethodGeneratorMsContextAction : SqlScriptIndexByMethodGeneratorContextActionBase
    {
        public SqlScriptIndexByMethodGeneratorMsContextAction(ICSharpContextActionDataProvider provider) : base(provider, GetIndexScript)
        {
        }

        public static string GetIndexScript(string tableName, string[] propertyNames)
        {
            var indexName = $"IX_{tableName}_{string.Join("_", propertyNames)}";
            return $"IF NOT EXISTS(SELECT* FROM sys.indexes WHERE NAME = '{indexName}' AND object_id = OBJECT_ID('{tableName}'))\r\n" +
                   $"   CREATE INDEX[{indexName}] ON[{tableName}] ({string.Join(", ", propertyNames.Select(x => $"[{x}]"))})\r\n" +
                   "   with(online = on)\r\n" +
                   "GO";
        }

        public override string Text => "Generate ms sql index script";
    }
}