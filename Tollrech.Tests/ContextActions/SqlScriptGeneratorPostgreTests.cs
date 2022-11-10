using NUnit.Framework;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptGeneratorPostgreTests : ContextActionBaseTests<SqlScriptGeneratorPostgreContextAction>
    {
        protected override string ExtraPath => "SqlScriptGeneratorPostgreTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlScriptGeneratorPostgreTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}