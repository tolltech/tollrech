using NUnit.Framework;
using Tollrech.Case;

namespace Tollrech.Tests.ContextActions
{
    public class CaseChangerTests : ContextActionBaseTests<DefaultCaseChangerContextAction>
    {
        protected override string ExtraPath => "CaseChangerTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "CaseChangerTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}