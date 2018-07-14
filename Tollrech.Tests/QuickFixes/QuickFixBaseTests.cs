using System.IO;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using IContextBoundSettingsStore = JetBrains.Application.Settings.IContextBoundSettingsStore;

namespace Tollrech.Tests.QuickFixes
{
    [TestFixture]
    [TestNetFramework46]
    public abstract class QuickFixBaseTests<TQuickFix> : CSharpQuickFixTestBase<TQuickFix> where TQuickFix : IQuickFix
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
                .GetFiles(Path.Combine(@".\Test\Data\", RelativeTestDataPath), "*.cs")
                .Select(Path.GetFileName)
                .Select(f => new TestCaseData(f))
                .ToArray();
        }
    }
}