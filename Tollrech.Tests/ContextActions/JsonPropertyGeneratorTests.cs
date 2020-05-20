using NUnit.Framework;
using Tollrech.Json;

namespace Tollrech.Tests.ContextActions
{
    public class JsonPropertyGeneratorTests : ContextActionBaseTests<JsonPropertyContextAction>
    {
        protected override string ExtraPath => "JsonPropertyGeneratorTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "JsonPropertyGeneratorTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}