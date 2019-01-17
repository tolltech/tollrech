using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;
using IReferenceExpression = JetBrains.ReSharper.Psi.CSharp.Tree.IReferenceExpression;

namespace Tollrech.EFClass
{
    //IF NOT EXISTS(SELECT* FROM sys.indexes WHERE NAME = 'IX_OnlinePaymentSession_State_Date' AND object_id = OBJECT_ID('OnlinePaymentSession'))
    //    CREATE INDEX[IX_OnlinePaymentSession_State_Date] ON[OnlinePaymentSession] ([State], [Date])
    //    with(online = on)
    //GO

    //public Task<OnlinePaymentSessionDbo[]> SelectAsync(DateTime exclusiveFromDate, PaymentState[] states, DateTime? exclusiveToDate = null)
    //{
    //var query = GetTable()
    //        .Where(x => states.Contains(x.State))
    //        .Where(x => x.Date > exclusiveFromDate);

    //    if (exclusiveToDate.HasValue)
    //{
    //    query = query.Where(x => x.Date < exclusiveToDate.Value);
    //}

    //return query.OrderBy(x => x.Date).ToArrayAsync();
    //}

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

        [NotNull]
        private string GetIndexScript()
        {
            var sb = new StringBuilder();
            var lambdas = methodDeclaration.Body.GetAllDescendants(sb).OfType<ILambdaExpression>().ToArray();

            var r = "";
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

                var parameterType = parameterDeclaration.DeclaredElement.Type;
                var parameterScalarType = parameterDeclaration.DeclaredElement.Type.GetScalarType();
                var parameterTypeElement = parameterScalarType.GetTypeElement();

                var file = parameterTypeElement.GetSingleOrDefaultSourceFile();
                var attribures1 = parameterTypeElement.GetAttributeInstances(true).ToArray();
                var attribures2 = parameterTypeElement.GetAttributeInstances(false).ToArray();

                var context = parameterTypeElement.GetResolveContext();
                var resolveResult = parameterScalarType.Resolve();

                var declarations = resolveResult.DeclaredElement?.GetDeclarations().ToArray() ?? Array.Empty<IDeclaration>();
                var classDeclaration1 = declarations.FirstOrDefault()?.GetAllDescendants().OfType<IClassDeclaration>();

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

            var indexName = $"IX_OnlinePaymentSession_{string.Join("_", distinctedPropertyNames)}";
            return $"IF NOT EXISTS(SELECT* FROM sys.indexes WHERE NAME = '{indexName}' AND object_id = OBJECT_ID('OnlinePaymentSession'))" +
                   $"   CREATE INDEX[{indexName}] ON[OnlinePaymentSession] ({string.Join(", ", distinctedPropertyNames.Select(x => $"[{x}]"))})" +
                   "   with(online = on)" +
                   "GO";
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