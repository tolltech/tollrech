using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlRepositoryGenerator", Description = "Generate Sql repository for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlRepositoryGeneratorContextAction : ContextActionBase
    {
        private readonly IClassDeclaration classDeclaration;
        private readonly CSharpElementFactory factory;
        private IAttribute tableAttribute;

        public SqlRepositoryGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>(); ;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            throw new NotImplementedException();
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table);
            return tableAttribute != null;
        }

        public override string Text => "Generate repository classes";
    }
}