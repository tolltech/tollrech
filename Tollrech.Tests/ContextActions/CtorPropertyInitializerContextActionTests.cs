using NUnit.Framework;
using Tollrech.Factoring;

namespace Tollrech.Tests.ContextActions
{
    public class CtorPropertyInitializerContextActionTests : ContextActionBaseTests<CtorPropertyInitializerContextAction>
    {
        protected override string ExtraPath => "CtorPropertyInitializerContextActionTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "CtorPropertyInitializerContextActionTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}