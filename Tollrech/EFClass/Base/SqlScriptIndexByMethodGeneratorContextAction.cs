using System;
using System.Collections.Generic;
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
using IReferenceExpression = JetBrains.ReSharper.Psi.CSharp.Tree.IReferenceExpression;

namespace Tollrech.EFClass
{
    public abstract class SqlScriptIndexByMethodGeneratorContextActionBase : ContextActionBase
    {
        private readonly Func<string, string[], string> getIndexScript;
        private readonly CSharpElementFactory factory;
        private readonly IMethodDeclaration methodDeclaration;
        private readonly IClassDeclaration classDeclaration;
        private IInvocationExpression[] invocationsExpressions;

        protected SqlScriptIndexByMethodGeneratorContextActionBase(ICSharpContextActionDataProvider provider, Func<string, string[], string> getIndexScript)
        {
            this.getIndexScript = getIndexScript;
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

        [NotNull]
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

            var distinctPropertyNames = indexProperties.Distinct().ToArray();
            var realTableName = tableName ?? "TODOTableName";
            return getIndexScript(realTableName, distinctPropertyNames);
        }

        [CanBeNull]
        private static string GetTableNameFromAttribute([NotNull] ILambdaParameterDeclaration parameterDeclaration)
        {
            var parameterScalarType = parameterDeclaration.DeclaredElement.Type.GetScalarType();
            var resolveResult = parameterScalarType?.Resolve();
            var declarations = resolveResult?.DeclaredElement?.GetDeclarations().ToArray() ?? Array.Empty<IDeclaration>();
            var classDeclaration = declarations.OfType<IAttributesOwnerDeclaration>().FirstOrDefault();
            var tableAttribute = classDeclaration?.Attributes.FindAttribute(Constants.Table, Constants.PostgreSqlTable);
            return tableAttribute?.Arguments.FirstOrDefault()?.Value?.ConstantValue.Value?.ToString();
        }

        public override string Text => "Generate sql index script";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            const string handlerStr = "Handler";
            var classIsHandler = classDeclaration?.NameIdentifier?.Name.Contains(handlerStr) ?? false;
            var baseTypeIsHandler = classDeclaration?.GetAllSuperTypes().Any(x => x.GetClrName().ShortName.Contains(handlerStr)) ?? false;

            invocationsExpressions = methodDeclaration?.Body?.GetAllDescendants().OfType<IInvocationExpression>().ToArray() ?? Array.Empty<IInvocationExpression>();
            var getTablePresented = invocationsExpressions.Any(x => (x.InvokedExpression as IReferenceExpression)?.NameIdentifier.Name == "GetTable");

            return methodDeclaration != null && (classIsHandler || baseTypeIsHandler || getTablePresented);
        }
    }
}