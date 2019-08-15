using NUnit.Framework;
using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptIndexByMethodGeneratorTests : ContextActionBaseTests<SqlScriptIndexByMethodGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlScriptIndexByMethodGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlScriptIndexByMethodGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}