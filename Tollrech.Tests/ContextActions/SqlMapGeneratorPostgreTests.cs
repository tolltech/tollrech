using NUnit.Framework;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.Tests.ContextActions
{
    public class SqlMapGeneratorPostgreTests : ContextActionBaseTests<SqlMapGeneratorPostgreContextAction>
    {
        protected override string ExtraPath => "SqlMapGeneratorPostgreTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlMapGeneratorPostgreTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}