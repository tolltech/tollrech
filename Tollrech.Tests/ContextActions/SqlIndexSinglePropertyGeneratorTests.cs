using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlIndexSinglePropertyGeneratorTests : ContextActionBaseTests<SqlScriptIndexGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlIndexSinglePropertyGeneratorTests";
        protected override string RelativeTestDataPath => "SqlIndexSinglePropertyGeneratorTests";
    }
}