using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass.Base
{
    public abstract class SqlScriptIndexGeneratorContextActionBase : ContextActionBase
    {
        private readonly Func<(string tableName, string columnName), string> GenerateSqlIndex;
        private readonly CSharpElementFactory factory;
        private readonly IClassDeclaration classDeclaration;
        private readonly IPropertyDeclaration propertyDeclaration;
        private IAttribute propertyColumnAttribute;
        private IAttribute tableAttribute;

        protected SqlScriptIndexGeneratorContextActionBase(ICSharpContextActionDataProvider provider, Func<(string tableName, string columnName), string> generateSqlIndex)
        {
            GenerateSqlIndex = generateSqlIndex;
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>();
            propertyColumnAttribute = propertyDeclaration?.Attributes.FindAttribute(Constants.Column);
            tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table, Constants.PostgreSqlTable);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var text = GetIndexScript();
            propertyDeclaration.AddXmlComment(text, factory);

            return null;
        }

        private string GetIndexScript()
        {
            var columnName = propertyColumnAttribute?.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOColumnName";
            var tableName = tableAttribute?.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";
            return GenerateSqlIndex((tableName, columnName));
        }

        public override string Text => "Generate sql index script";

        public override bool IsAvailable(IUserDataHolder cache)
        {

            return tableAttribute != null && propertyColumnAttribute != null;
        }
    }
}