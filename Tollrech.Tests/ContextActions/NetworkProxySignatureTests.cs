using NUnit.Framework;
using Tollrech.NetworkProxy;

namespace Tollrech.Tests.ContextActions
{
    public class NetworkProxySignatureTests : ContextActionBaseTests<NetworkProxySignatureContextAction>
    {
        protected override string ExtraPath => "NetworkProxySignatureTests";
        protected override string RelativeTestDataPath => relativeTestDataPath;

        private const string relativeTestDataPath = "NetworkProxySignatureTests";

        [TestCaseSource(nameof(FileNames), new object[] {relativeTestDataPath})]
        public override void TestAbstract(string fileName)
        {
            Test(fileName);
        }
    }
}