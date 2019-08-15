using NUnit.Framework;
using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptGeneratorTests : ContextActionBaseTests<SqlScriptGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlScriptGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlScriptGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}