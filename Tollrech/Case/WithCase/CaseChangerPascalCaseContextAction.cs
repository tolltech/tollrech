﻿using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Tollrech.Case.Base;
using Tollrech.Common;

namespace Tollrech.Case.WithCase
{
	[ContextAction(Name = "AddJsonPropertyPascalCase", Description = "Generate JsonProperty attributes for class-entity with PascalCase names", Group = "C#", Disabled = true, Priority = 1)]
	public class CaseChangerPascalCaseContextAction : CaseContextActionBase
	{
		public CaseChangerPascalCaseContextAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider, InflectorExtensions.Pascalize)
		{
		}

		public override string Text { get; } = "To PascalCase";
	}
}