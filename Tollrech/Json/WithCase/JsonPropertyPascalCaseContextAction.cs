using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using Tollrech.Common;
using Tollrech.Json.Base;

namespace Tollrech.Json.WithCase
{
	[ContextAction(Name = "AddJsonPropertyPascalCase", Description = "Generate JsonProperty attributes for class-entity with PascalCase names", Group = "C#", Disabled = true, Priority = 1)]
	public class JsonPropertyPascalCaseContextAction : JsonPropertyContextActionBase
	{
		public JsonPropertyPascalCaseContextAction(ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Pascalize)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes PascalCase";
	}
}