using NUnit.Framework;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.Tests.ContextActions
{
    public class SqlIndexSinglePropertyGeneratorPostgreTests : ContextActionBaseTests<SqlScriptIndexGeneratorPostgreContextAction>
    {
        protected override string ExtraPath => "SqlIndexSinglePropertyGeneratorPostgreTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlIndexSinglePropertyGeneratorPostgreTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}