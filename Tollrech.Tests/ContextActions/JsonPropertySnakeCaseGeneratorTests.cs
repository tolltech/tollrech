using NUnit.Framework;
using Tollrech.Json.WithCase;

namespace Tollrech.Tests.ContextActions
{
    public class JsonPropertySnakeCaseGeneratorTests : ContextActionBaseTests<JsonPropertySnakeCaseContextAction>
    {
        protected override string ExtraPath => "JsonPropertySnakeCaseGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "JsonPropertySnakeCaseGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}