using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.Case.Base;
using Tollrech.Common;

namespace Tollrech.Case.WithCase
{
	[ContextAction(Name = "AddJsonPropertyCamelCase", Description = "Generate JsonProperty attributes for class-entity with camelCase names", Group = "C#", Disabled = true, Priority = 1)]
	public class CaseChangerCamelCaseContextAction : CaseContextActionBase
	{
		public CaseChangerCamelCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Camelize)
		{
		}

		public override string Text { get; } = "To camelCase";
	}
}