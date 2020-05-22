using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using Tollrech.Common;
using Tollrech.Json.Base;

namespace Tollrech.Json.WithCase
{
	[ContextAction(Name = "AddJsonPropertyKebabCase", Description = "Generate JsonProperty attributes for class-entity with kebab-case names", Group = "C#", Disabled = true, Priority = 1)]
	public class JsonPropertyKebabCaseContextAction : JsonPropertyContextActionBase
	{
		public JsonPropertyKebabCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Kebaberize)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes kebab-case";
	}
}