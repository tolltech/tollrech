using NUnit.Framework;
using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlRepositoryGeneratorTests : ContextActionBaseTests<SqlRepositoryGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlRepositoryGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlRepositoryGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] { relativeTestDataPath })]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}