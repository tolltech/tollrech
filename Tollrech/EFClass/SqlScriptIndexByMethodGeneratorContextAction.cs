using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;
using IReferenceExpression = JetBrains.ReSharper.Psi.CSharp.Tree.IReferenceExpression;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlScriptIndexByMethodGeneratorContextAction", Description = "Generate Sql script index for handler methods", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlScriptIndexByMethodGeneratorContextAction : ContextActionBase
    {
        private readonly CSharpElementFactory factory;
        private readonly IMethodDeclaration methodDeclaration;
        private readonly IClassDeclaration classDeclaration;
        private IInvocationExpression[] invocationsExpressions;

        public SqlScriptIndexByMethodGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            methodDeclaration = provider.GetSelectedElement<IMethodDeclaration>();
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var text = GetIndexScript();
            methodDeclaration.AddXmlComment(text, factory);

            return null;
        }

        private string GetIndexScript()
        {
            var lambdas = methodDeclaration.Body.GetAllDescendants().OfType<ILambdaExpression>().ToArray();

            string tableName = null;
            var lambdaExpressions = lambdas.Distinct().ToArray();
            var indexProperties = new List<string>(lambdaExpressions.Length);
            foreach (var lambdaExpression in lambdaExpressions)
            {
                var childNodes = lambdaExpression.GetAllDescendants().Distinct().ToArray();

                var parameterDeclaration = childNodes.OfType<ILambdaParameterDeclaration>().FirstOrDefault();
                if (parameterDeclaration == null)
                {
                    continue;
                }

                var currentTableName = GetTableNameFromAttribute(parameterDeclaration);
                if (tableName != null && tableName != currentTableName)
                {
                    continue;
                }

                tableName = currentTableName;

                var lambdaParameterName = parameterDeclaration.NameIdentifier.Name;
                var referenceExpressions = childNodes.OfType<IReferenceExpression>().Where(x => x.NameIdentifier.Name == lambdaParameterName).Distinct().ToArray();

                foreach (var referenceExpression in referenceExpressions)
                {
                    var propertyName = (referenceExpression?.Parent as IReferenceExpression)?.NameIdentifier.Name;
                    if (propertyName.IsNullOrWhitespace())
                    {
                        continue;
                    }

                    indexProperties.Add(propertyName);
                }
            }

            var distinctedPropertyNames = indexProperties.Distinct().ToArray();
            var realTableName = tableName ?? "TODOTableName";
            var indexName = $"IX_{realTableName}_{string.Join("_", distinctedPropertyNames)}";
            return $"IF NOT EXISTS(SELECT* FROM sys.indexes WHERE NAME = '{indexName}' AND object_id = OBJECT_ID('{realTableName}'))\r\n" +
                   $"   CREATE INDEX[{indexName}] ON[{realTableName}] ({string.Join(", ", distinctedPropertyNames.Select(x => $"[{x}]"))})\r\n" +
                   "   with(online = on)\r\n" +
                   "GO";
        }

        private static string GetTableNameFromAttribute(ILambdaParameterDeclaration parameterDeclaration)
        {
            const string tableNameAttribute = "Table";

            var parameterScalarType = parameterDeclaration.DeclaredElement.Type.GetScalarType();
            var resolveResult = parameterScalarType?.Resolve();
            var declarations = resolveResult?.DeclaredElement?.GetDeclarations().ToArray() ?? Array.Empty<IDeclaration>();
            var classDeclaration = declarations.OfType<IAttributesOwnerDeclaration>().FirstOrDefault();
            var tableAttribute = classDeclaration?.Attributes.FirstOrDefault(x => x.Name.NameIdentifier.Name == tableNameAttribute);
            return tableAttribute?.Arguments.FirstOrDefault()?.Value?.ConstantValue.Value?.ToString();
        }

        public override string Text => "Generate sql index script";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            const string handlerStr = "Handler";
            var classIsHandler = classDeclaration?.NameIdentifier?.Name.Contains(handlerStr) ?? false;
            var baseTypeIsHandler = classDeclaration?.GetAllSuperTypes().Any(x => x.GetClrName().ShortName.Contains(handlerStr)) ?? false;

            invocationsExpressions = methodDeclaration.Body.GetAllDescendants().OfType<IInvocationExpression>().ToArray();
            var getTablePresented = invocationsExpressions.Any(x => (x.InvokedExpression as IReferenceExpression)?.NameIdentifier.Name == "GetTable");

            return methodDeclaration != null && (classIsHandler || baseTypeIsHandler || getTablePresented);
        }
    }
}