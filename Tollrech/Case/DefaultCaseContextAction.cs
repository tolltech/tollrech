using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using Tollrech.Case.Base;
using Tollrech.Case.WithCase;

namespace Tollrech.Case
{
	[ContextAction(Name = "ChangeCase", Description = "Change case", Group = "C#", Disabled = false, Priority = 1)]
	public class DefaultCaseContextAction : CaseContextActionBase
	{
		private readonly ContextActionBase[] caseActions;

		public DefaultCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
		{
			caseActions = new ContextActionBase[]
			                        {
				                        new CasePascalCaseContextAction(provider),
				                        new CaseCamelCaseContextAction(provider),
				                        new CaseKebabCaseContextAction(provider),
				                        new CaseSnakeCaseContextAction(provider)
			                        };
		}

		public override string Text { get; } = "Add Case attributes";

		public override IEnumerable<IntentionAction> CreateBulbItems()
		{
			var bulbItems = new List<IntentionAction>();

			var mainAnchor = new SubmenuAnchor(IntentionsAnchors.ContextActionsAnchor, SubmenuBehavior.Executable);
			var thisAction = this.ToContextActionIntention(mainAnchor);

			bulbItems.Add(thisAction);
			bulbItems.AddRange(caseActions.Select(subAction => subAction.ToContextActionIntention(mainAnchor)));

			return bulbItems;
		}
	}
}