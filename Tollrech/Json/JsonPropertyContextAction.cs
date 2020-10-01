using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using Tollrech.Json.Base;
using Tollrech.Json.WithCase;

namespace Tollrech.Json
{
	[ContextAction(Name = "AddJsonProperty", Description = "Generate JsonProperty attributes for class-entity", Group = "C#", Disabled = false, Priority = 1)]
	public class JsonPropertyContextAction : JsonPropertyContextActionBase
	{
		private readonly ContextActionBase[] jsonActionsWithCasing;

		public JsonPropertyContextAction(ICSharpContextActionDataProvider provider) : base(provider)
		{
			jsonActionsWithCasing = new ContextActionBase[]
			                        {
				                        new JsonPropertyPascalCaseContextAction(provider),
				                        new JsonPropertyCamelCaseContextAction(provider),
				                        new JsonPropertyKebabCaseContextAction(provider),
				                        new JsonPropertySnakeCaseContextAction(provider)
			                        };
		}

		public override string Text { get; } = "Add JsonProperty attributes";

		public override IEnumerable<IntentionAction> CreateBulbItems()
		{
			var bulbItems = new List<IntentionAction>();

			var mainAnchor = new SubmenuAnchor(IntentionsAnchors.ContextActionsAnchor, SubmenuBehavior.Executable);
			var thisAction = this.ToContextActionIntention(mainAnchor);

			bulbItems.Add(thisAction);
			bulbItems.AddRange(jsonActionsWithCasing.Select(subAction => subAction.ToContextActionIntention(mainAnchor)));

			return bulbItems;
		}
	}
}