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
    [ContextAction(Name = "SqlScriptIndexGenerator", Description = "Generate Sql script index for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlScriptIndexGeneratorContextAction : ContextActionBase
    {
        private readonly CSharpElementFactory factory;
        private readonly IClassDeclaration classDeclaration;
        private readonly IPropertyDeclaration propertyDeclaration;
        private IAttribute propertyColumnAttribute;
        private IAttribute tableAttribute;

        public SqlScriptIndexGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();

            propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var text = GetIndexScript();
            propertyDeclaration.AddXmlComment(text, factory);

            throw new NotImplementedException();
        }

        private string GetIndexScript()
        {
            //var propertyInfo = propertyDeclaration.GetPropertyInfo();

            throw new NotImplementedException();
        }

        public override string Text => "Generate sql index script";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            propertyColumnAttribute = propertyDeclaration?.Attributes.FindAttribute(Constants.Column);
            tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table);
            return tableAttribute != null && propertyColumnAttribute != null;
        }
    }
}