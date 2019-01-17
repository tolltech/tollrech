using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptIndexByMethodGeneratorTests : ContextActionBaseTests<SqlScriptIndexByMethodGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlScriptIndexByMethodGeneratorTests";
        protected override string RelativeTestDataPath => "SqlScriptIndexByMethodGeneratorTests";
    }
}