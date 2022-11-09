using NUnit.Framework;
using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlIndexSinglePropertyGeneratorTests : ContextActionBaseTests<DefaultSqlScriptIndexGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlIndexSinglePropertyGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlIndexSinglePropertyGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}