using System.IO;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace Tollrech.Tests.ContextActions
{
    [TestFixture]
    [TestNetFramework46]
    public abstract class ContextActionBaseTests<TQuickFix> : CSharpContextActionExecuteTestBase<TQuickFix> where TQuickFix : class, IContextAction
    {
        [TestCaseSource(nameof(FileNames))]
        public void Test(string fileName)
        {
            ExecuteWithinSettingsTransaction(store => TestTransaction(store, fileName));
        }

        private void TestTransaction(IContextBoundSettingsStore store, string fileName)
        {
            ConfigureSettings(store);

            DoTestFiles(fileName);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ConfigureSettings(IContextBoundSettingsStore store)
        {
            /*
             *
             * example:
             *
             * store.SetValue((CSharpLanguageProjectSettings s) => s.LanguageLevel, CSharpLanguageLevel.CSharp70);
             *
             */
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected TestCaseData[] FileNames()
        {
            return Directory
                .GetFiles(Path.Combine(@"..\..\..\Test\Data\", RelativeTestDataPath), "*.cs")
                .Select(Path.GetFileName)
                .Select(f => new TestCaseData(f))
                .ToArray();
        }
    }
}