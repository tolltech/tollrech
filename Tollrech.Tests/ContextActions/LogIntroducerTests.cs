using JetBrains.ReSharper.TestFramework;
using Tollrech.Logging;

namespace Tollrech.Tests.ContextActions
{
    [TestPackages("log4net")]
    public class LogIntroducerTests : ContextActionBaseTests<LogIntroducerContextAction>
    {
        protected override string ExtraPath => "LogIntroducerTests";
        protected override string RelativeTestDataPath => "LogIntroducerTests";
        public override void TestAbstract(string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}