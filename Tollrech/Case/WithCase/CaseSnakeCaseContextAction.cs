using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.Case.Base;
using Tollrech.Common;

namespace Tollrech.Case.WithCase
{
	[ContextAction(Name = "AddJsonPropertySnakeCase", Description = "Generate JsonProperty attributes for class-entity with snake_case names", Group = "C#", Disabled = true, Priority = 1)]
	public class CaseSnakeCaseContextAction : CaseContextActionBase
	{
		public CaseSnakeCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Underscore)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes snake_case";
	}
}