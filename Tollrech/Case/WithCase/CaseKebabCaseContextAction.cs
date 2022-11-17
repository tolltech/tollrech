using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.Case.Base;
using Tollrech.Common;

namespace Tollrech.Case.WithCase
{
	[ContextAction(Name = "AddJsonPropertyKebabCase", Description = "Generate JsonProperty attributes for class-entity with kebab-case names", Group = "C#", Disabled = true, Priority = 1)]
	public class CaseKebabCaseContextAction : CaseContextActionBase
	{
		public CaseKebabCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Kebaberize)
		{
		}

		public override string Text { get; } = "Add JsonProperty attributes kebab-case";
	}
}