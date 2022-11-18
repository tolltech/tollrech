using NUnit.Framework;
using Tollrech.Case.WithCase;

namespace Tollrech.Tests.ContextActions
{
    public class CaseChangerSnakeCaseTests : ContextActionBaseTests<CaseChangerSnakeCaseContextAction>
    {
        protected override string ExtraPath => "CaseChangerSnakeCaseTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "CaseChangerSnakeCaseTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}