using NUnit.Framework;
using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlMapGeneratorTests : ContextActionBaseTests<SqlMapGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlMapGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlMapGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}