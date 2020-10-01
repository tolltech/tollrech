using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using Tollrech.Common;
using Tollrech.Json.Base;

namespace Tollrech.Json.WithCase
{
	[ContextAction(Name = "AddJsonPropertyCamelCase", Description = "Generate JsonProperty attributes for class-entity with camelCase names", Group = "C#", Disabled = true, Priority = 1)]
	public class JsonPropertyCamelCaseContextAction : JsonPropertyContextActionBase
	{
		public JsonPropertyCamelCaseContextAction(ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Camelize)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes camelCase";
	}
}