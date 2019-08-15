using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using Tollrech.Logging;

namespace Tollrech.Tests.ContextActions
{
    [TestPackages("log4net")]
    public class LogIntroducerTests : ContextActionBaseTests<LogIntroducerContextAction>
    {
        protected override string ExtraPath => "LogIntroducerTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "LogIntroducerTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}