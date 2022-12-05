using NUnit.Framework;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptIndexByMethodGeneratorPostgreTests : ContextActionBaseTests<SqlScriptIndexByMethodGeneratorPostgreContextAction>
    {
        protected override string ExtraPath => "SqlScriptIndexByMethodGeneratorPostgreTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "SqlScriptIndexByMethodGeneratorPostgreTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}