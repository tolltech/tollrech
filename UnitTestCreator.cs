using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Extension;

namespace Tollrech
{
    [QuickFix]
    public class MockedClassCreateFix : QuickFixBase
    {
        private readonly IncorrectArgumentNumberError error;
        private const string QueryExecutroFactoryInterfaceName = "IQueryExecutorFactory";

        public MockedClassCreateFix(IncorrectArgumentNumberError error)
        {
            this.error = error;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var ctorTreeNode = error?.Reference.GetTreeNode();
            var ctorExpression = ctorTreeNode as IObjectCreationExpression;
            var ctor = error?.Reference?.CurrentResolveResult?.DeclaredElement as IConstructor;
            if (ctor == null || ctorExpression == null)
                return null;

            var ctorParams = ctor.Parameters;
            if (ctorParams.Count == 0)
                return null;

            var factory = CSharpElementFactory.GetInstance(error.Reference.GetAccessContext().GetPsiModule());

            var methodDeclaration = ctorTreeNode.FindParent<IMethodDeclaration>();
            var classDeclaration = methodDeclaration?.FindParent<IClassDeclaration>();

            var mockInfos = new List<MockInfo>();
            for (var i = 0; i < ctorParams.Count; ++i)
            {
                var ctorParam = ctorParams[i];
                var isArray = ctorParam.Type is IArrayType;

                if (!ctorParam.Type.IsInterfaceType() && !(isArray && ctorParam.Type.GetScalarType().IsInterfaceType())
                    || ctorParam.Type.GetScalarType().GetInterfaceType()?.ShortName == QueryExecutroFactoryInterfaceName)
                    continue;

                var ctorParamName = ctorParam.ShortName;
                if (isArray)
                {
                    var scalarType = ctorParam.Type.GetScalarType().GetInterfaceType();
                    var singleName = ctorParamName.EndsWith("es") ? ctorParamName.RemoveEnd("es") : ctorParamName.RemoveEnd("s");

                    var arrayParamNames = Enumerable.Range(1, 2).Select(x => $"{singleName}{x}").ToArray();
                    foreach (var arrayParamName in arrayParamNames)
                    {
                        var expression = factory.CreateExpression("NewMock<$0>();", scalarType.ShortName);
                        mockInfos.Add(new MockInfo
                        {
                            Statement = factory.CreateStatement("$0 = $1;", arrayParamName, expression),
                            Type = ((IArrayType) ctorParam.Type).ElementType,
                            Name = arrayParamName
                        });
                    }

                    mockInfos.Add(new MockInfo
                    {
                        Statement = factory.CreateStatement("$0 = $1;", ctorParam.ShortName, factory.CreateExpression($"new[] {{ {string.Join(", ", arrayParamNames)} }}")),
                        Type = ctorParam.Type,
                        Name = ctorParam.ShortName
                    });
                }
                else
                {
                    mockInfos.Add(new MockInfo
                    {
                        Statement = factory.CreateStatement("$0 = $1;", ctorParam.ShortName, factory.CreateExpression("NewMock<$0>();", ctorParam.Type)),
                        Type = ctorParam.Type,
                        Name = ctorParam.ShortName
                    });
                }
            }

            var ctorStatement = methodDeclaration.Body.Statements.FirstOrDefault(x => (x as IExpressionStatement)?.Expression == ctorExpression);
            foreach (var mockInfo in mockInfos)
            {
                classDeclaration.AddClassMemberDeclaration(factory.CreateFieldDeclaration(mockInfo.Type, mockInfo.Name));
                methodDeclaration.Body.AddStatementBefore(mockInfo.Statement, ctorStatement);
            }

            var superTypes = classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes).Select(x => x.GetClassType()).Where(x => x != null).ToArray();
            var names = ctorParams.Select(x => GetCtorArgumentName(x, superTypes));
            var objArgExpressions = new object[] { ctor.GetContainingType() }.Concat(names).ToArray();
            var argumentsPattern = string.Join(", ", Enumerable.Range(1, ctorParams.Count).Select(x => $"${x}"));
            var newExpression = factory.CreateExpression($"new $0({argumentsPattern});", objArgExpressions);

            ctorExpression.ReplaceBy(newExpression);

            return null;
        }

        private class MockInfo
        {
            public IType Type { get; set; }
            public string Name { get; set; }
            public ICSharpStatement Statement { get; set; }
        }

        private static string GetCtorArgumentName(IParameter ctorParam, IClass[] superTypes)
        {
            if (!ctorParam.Type.GetScalarType().IsInterfaceType())
                return "TODO";

            const string queryExecutorFactoryFieldName = "QueryExecutorFactory";
            if (ctorParam.Type.GetInterfaceType()?.ShortName == QueryExecutroFactoryInterfaceName
                    && superTypes.Any(x => x.Properties.Any(y => y.ShortName == queryExecutorFactoryFieldName)
                                        || x.Fields.Any(y => y.ShortName == queryExecutorFactoryFieldName))
                )
                return queryExecutorFactoryFieldName;

            return ctorParam.ShortName;
        }

        public override string Text => "Create mocked class";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(error?.Reference?.CurrentResolveResult?.DeclaredElement is IConstructor))
                return false;

            var ctorTreeNode = error?.Reference.GetTreeNode();
            var classDeclaration = ctorTreeNode.FindParent<IClassDeclaration>();
            if (classDeclaration == null)
                return false;

            var superTypes = classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes);
            return superTypes.Any(x => x.GetClassType()?.Methods.Any(y => y.ShortName == "NewMock") ?? false);
        }
    }
}