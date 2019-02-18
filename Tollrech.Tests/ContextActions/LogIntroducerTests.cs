using Tollrech.Logging;

namespace Tollrech.Tests.ContextActions
{
    public class LogIntroducerTests : ContextActionBaseTests<LogIntroducerContextAction>
    {
        protected override string ExtraPath => "LogIntroducerTests";
        protected override string RelativeTestDataPath => "LogIntroducerTests";
    }
}