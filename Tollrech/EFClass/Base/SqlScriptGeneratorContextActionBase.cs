using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass.Base
{
    public abstract class SqlScriptGeneratorContextActionBase : ContextActionBase
    {
        private readonly IClassDeclaration classDeclaration;
        private readonly IPropertyDeclaration propertyDeclaration;
        private readonly CSharpElementFactory factory;
        private IAttribute propertyColumnAttribute;
        private IAttribute tableAttribute;

        protected SqlScriptGeneratorContextActionBase(ICSharpContextActionDataProvider provider,
                                                      [NotNull] Func<(string TableName, PropertyInfo PropertyInfo), string> generateCustomPropertyScript,
                                                      [NotNull] Func<(string TableName, PropertyInfo[] PropertyInfos), string> generateCustomTableScript
            )
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            ;
            propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>();
            GenerateCustomPropertyScript = generateCustomPropertyScript;
            GenerateCustomTableScript = generateCustomTableScript;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            (string Script, ITypeMemberDeclaration Declaration) result;
            if (propertyColumnAttribute != null)
            {
                result.Script = GeneratePropertyScript();
                result.Declaration = propertyDeclaration;
            }
            else if (tableAttribute != null)
            {
                result.Script = GenerateTableScript(tableAttribute);
                result.Declaration = classDeclaration;
            }
            else
            {
                return null;
            }

            result.Declaration.AddXmlComment(result.Script, factory);
            return null;
        }

        [NotNull]
        private string GeneratePropertyScript()
        {
            var propertyInfo = propertyDeclaration.GetPropertyInfo();
            var tableName = tableAttribute.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";

            return GenerateCustomPropertyScript((tableName, propertyInfo));
        }

        [NotNull]
        private readonly Func<(string TableName, PropertyInfo PropertyInfo), string> GenerateCustomPropertyScript;

        [NotNull]
        private string GenerateTableScript([NotNull] IAttribute attribute)
        {
            var tableName = attribute.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOTableName";
            var properties = classDeclaration.PropertyDeclarations
                .Where(x => x.HasGetSet())
                .Where(x => x.Attributes.FindAttribute(Constants.Column) != null)
                .Select(x => x.GetPropertyInfo())
                .ToArray();

            return GenerateCustomTableScript((tableName, properties));
        }

        [NotNull]
        private readonly Func<(string TableName, PropertyInfo[] PropertyInfos), string> GenerateCustomTableScript;

        public override string Text => "Generate sql script";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            propertyColumnAttribute = propertyDeclaration?.Attributes.FindAttribute(Constants.Column);
            tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table);
            return tableAttribute != null;
        }
    }
}