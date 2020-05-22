using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using Tollrech.Common;
using Tollrech.Json.Base;

namespace Tollrech.Json.WithCase
{
	[ContextAction(Name = "AddJsonPropertySnakeCase", Description = "Generate JsonProperty attributes for class-entity with snake_case names", Group = "C#", Disabled = true, Priority = 1)]
	public class JsonPropertySnakeCaseContextAction : JsonPropertyContextActionBase
	{
		public JsonPropertySnakeCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Underscore)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes snake_case";
	}
}