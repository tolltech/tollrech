using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using Tollrech.EFClass.Base;
using Tollrech.EFClass.SpecialDb;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlScriptGenerator", Description = "Generate Sql script for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class DefaultSqlScriptGeneratorContextAction : SqlScriptGeneratorContextActionBase
    {
        [NotNull, ItemNotNull]
        private readonly ContextActionBase[] specialDbScriptActions;

        public DefaultSqlScriptGeneratorContextAction(ICSharpContextActionDataProvider provider)
            : base(provider,
                SqlScriptGeneratorMsContextAction.GenerateCustomPropertyScript,
                SqlScriptGeneratorMsContextAction.GenerateCustomTableScript)
        {
            specialDbScriptActions = new ContextActionBase[]
                                     {
                                         new SqlScriptGeneratorMsContextAction(provider),
                                         new SqlScriptGeneratorPostgreContextAction(provider)
                                     };
        }

        public override string Text => "Generate sql script";

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