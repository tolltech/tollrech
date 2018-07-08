using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlMapGeneratorTests : ContextActionBaseTests<SqlMapGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlMapGeneratorTests";
        protected override string RelativeTestDataPath => "SqlMapGeneratorTests";
    }
}