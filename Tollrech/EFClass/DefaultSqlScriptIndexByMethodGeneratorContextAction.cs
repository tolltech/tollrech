using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlScriptIndexByMethodGeneratorContextAction", Description = "Generate Sql script index for handler methods", Group = "C#", Disabled = false, Priority = 1)]
    public class DefaultSqlScriptIndexByMethodGeneratorContextAction : SqlScriptIndexByMethodGeneratorContextActionBase
    {
        [NotNull, ItemNotNull]
        private readonly ContextActionBase[] specialDbScriptActions;

        public DefaultSqlScriptIndexByMethodGeneratorContextAction(ICSharpContextActionDataProvider provider) : base(provider, SqlScriptIndexByMethodGeneratorMsContextAction.GetIndexScript)
        {
            specialDbScriptActions = new ContextActionBase[]
                                     {
                                         new SqlScriptIndexByMethodGeneratorMsContextAction(provider),
                                         new SqlScriptIndexByMethodGeneratorPostgreContextAction(provider),
                                     };
        }

        public override string Text => "Generate sql index script";

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var bulbItems = new List<IntentionAction>();

            var mainAnchor = new SubmenuAnchor(IntentionsAnchors.ContextActionsAnchor, SubmenuBehavior.Executable);
            var thisAction = this.ToContextActionIntention(mainAnchor);

            bulbItems.Add(thisAction);
            bulbItems.AddRange(specialDbScriptActions.Select(subAction => subAction.ToContextActionIntention(mainAnchor)));

            return bulbItems;
        }

    }
}