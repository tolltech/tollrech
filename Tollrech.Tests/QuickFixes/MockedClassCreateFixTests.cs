using NUnit.Framework;
using Tollrech.UnitTestMocks;

namespace Tollrech.Tests.QuickFixes
{
    public class MockedClassCreateFixTests : QuickFixBaseTests<MockedClassCreateFix>
    {
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "MockedClassCreateFixTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}