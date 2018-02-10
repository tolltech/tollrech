using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Extension;

namespace Tollrech.UnitTestMocks
{
    [QuickFix]
    public class MockedClassCreateFix : QuickFixBase
    {
        private readonly IncorrectArgumentNumberError error;
        private const string QueryExecutroFactoryInterfaceName = "IQueryExecutorFactory";
        private ITypeConversionRule cSharpTypeConversionRule;

        private IConstructor ctor;
        private IClassDeclaration classDeclaration;
        private IDeclaredType[] superTypes;

        public MockedClassCreateFix(IncorrectArgumentNumberError error)
        {
            this.error = error;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var ctorTreeNode = error?.Reference.GetTreeNode();
            var ctorExpression = ctorTreeNode as IObjectCreationExpression;
            if (ctor == null || ctorExpression == null)
            {
                return null;
            }

            var ctorParams = ctor.Parameters;
            if (ctorParams.Count == 0)
            {
                return null;
            }

            var psiModule = error.Reference.GetAccessContext().GetPsiModule();
            cSharpTypeConversionRule = psiModule.GetTypeConversionRule();
            var factory = CSharpElementFactory.GetInstance(psiModule);

            var methodDeclaration = ctorTreeNode.FindParent<IMethodDeclaration>();

            var existedArguments = ctorExpression.AllArguments(false)
                .Select(x => new ArgumentInfo
                {
                    Type = x.GetExpressionType()?.ToIType(),
                    Expression = (x as ICSharpArgument)?.Value,
                    IsCSharpArgument = x is ICSharpArgument
                })
                .ToArray();

            var superClassTypes = superTypes.Select(x => x.GetClassType()).Where(x => x != null).ToArray();

            var mockInfos = GenerateNewMockInfos(ctorParams, superClassTypes, existedArguments, factory);
            AddMocksToClassDeclaration(methodDeclaration, ctorExpression, mockInfos, classDeclaration, factory);

            var argExpressions = GetCtorArgumentExpressions(existedArguments, ctorParams, superClassTypes);
            var argumentsPattern = string.Join(", ", Enumerable.Range(1, ctorParams.Count).Select(x => $"${x}"));
            var newExpression = factory.CreateExpression($"new $0({argumentsPattern});", argExpressions.ToArray());

            ctorExpression.ReplaceBy(newExpression);

            return null;
        }

        private object[] GetCtorArgumentExpressions(ArgumentInfo[] existedArguments, IList<IParameter> ctorParams, IClass[] superClassTypes)
        {
            var argExpressions = new List<object> { ctor.GetContainingType() };
            var existedArgumentsByType =
                existedArguments.Where(x => x.Expression != null).GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var ctorParam in ctorParams)
            {
                var argumentType = ctorParam.Type;

                if (!existedArgumentsByType.ContainsKey(argumentType))
                {
                    var possibleArgument = existedArguments.FirstOrDefault(x => x.Type.IsImplicitlyConvertibleTo(argumentType, cSharpTypeConversionRule));
                    if (possibleArgument != null)
                    {
                        argumentType = possibleArgument.Type;
                    }
                }

                if (existedArgumentsByType.ContainsKey(argumentType))
                {
                    var argument = existedArgumentsByType[argumentType].InfinitivePop();
                    argExpressions.Add(argument.Expression);
                }
                else
                {
                    argExpressions.Add(GetCtorArgumentName(ctorParam.Type, ctorParam.ShortName, superClassTypes));
                }
            }
            return argExpressions.ToArray();
        }

        private static void AddMocksToClassDeclaration(IMethodDeclaration methodDeclaration, IObjectCreationExpression ctorExpression, MockInfo[] mockInfos, IClassDeclaration classDeclaration, CSharpElementFactory factory)
        {
            var ctorStatement = methodDeclaration.Body.Statements
               .FirstOrDefault(x =>
               {
                   var expression = (x as IExpressionStatement)?.Expression;
                   if (expression != null && (expression == ctorExpression || (expression as IAssignmentExpression)?.Source == ctorExpression))
                   {
                       return true;
                   }

                   var declarationStatement = (x as IDeclarationStatement);
                   if (declarationStatement?.VariableDeclarations.Any(varDeclaration => (varDeclaration.Initial as IExpressionInitializer)?.Value == ctorExpression) ?? false)
                   {
                       return true;
                   }

                   return false;
               });

            foreach (var mockInfo in mockInfos)
            {
                if (classDeclaration.MemberDeclarations.All(x => x.DeclaredName != mockInfo.Name))
                {
                    classDeclaration.AddClassMemberDeclaration(factory.CreateFieldDeclaration(mockInfo.Type, mockInfo.Name));
                }

                var elementHasAssigned = methodDeclaration.Body.Statements.Any(x =>
                {
                    var assignmentOperands = ((x as IExpressionStatement)?.Expression as IAssignmentExpression)?.OperatorOperands;
                    return assignmentOperands != null && assignmentOperands.Any(operand => (operand as IReferenceExpression)?.NameIdentifier.Name == mockInfo.Name);
                });

                if (!elementHasAssigned)
                {
                    methodDeclaration.Body.AddStatementBefore(mockInfo.Statement, ctorStatement);
                }
            }
        }

        private MockInfo[] GenerateNewMockInfos(IList<IParameter> ctorParams, IClass[] superClassTypes, ArgumentInfo[] existedArguments, CSharpElementFactory factory)
        {
            var mockInfos = new List<MockInfo>();
            foreach (var ctorParam in ctorParams)
            {
                var isArray = ctorParam.Type is IArrayType;
                var interfaceType = ctorParam.Type.GetScalarType().GetInterfaceType();

                if (!ctorParam.Type.IsInterfaceType() && !(isArray && interfaceType != null)
                    || ParamIsQeuryExecutorFactoryAndAvailable(ctorParam.Type, superClassTypes))
                {
                    continue;
                }

                if (existedArguments.Where(x => x.IsCSharpArgument).Any(x => x.Type.IsImplicitlyConvertibleTo(ctorParam.Type, cSharpTypeConversionRule)))
                {
                    continue;
                }

                var ctorParamName = ctorParam.ShortName;
                if (isArray)
                {
                    var scalarType = interfaceType;
                    var singleName = GetSingleName(ctorParamName);

                    var arrayParamNames = Enumerable.Range(1, 2).Select(x => $"{singleName}{x}").ToArray();
                    foreach (var arrayParamName in arrayParamNames)
                    {
                        var expression = factory.CreateExpression("NewMock<$0>();", scalarType.ShortName);
                        mockInfos.Add(new MockInfo
                        {
                            Statement = factory.CreateStatement("$0 = $1;", arrayParamName, expression),
                            Type = ((IArrayType)ctorParam.Type).ElementType,
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
            return mockInfos.ToArray();
        }

        private static string GetSingleName(string ctorParamName)
        {
            var singleName = ctorParamName.EndsWith("es") && !ctorParamName.EndsWith("ervices") ? ctorParamName.RemoveEnd("es") : ctorParamName.RemoveEnd("s");
            return singleName;
        }

        private const string queryExecutorFactoryFieldName = "QueryExecutorFactory";
        private static string GetCtorArgumentName(IType ctorParamType, string shortName, IClass[] superTypes)
        {
            if (!ctorParamType.GetScalarType().IsInterfaceType())
            {
                return "TODO";
            }

            if (ParamIsQeuryExecutorFactoryAndAvailable(ctorParamType, superTypes))
            {
                return queryExecutorFactoryFieldName;
            }

            return shortName;
        }

        private static bool ParamIsQeuryExecutorFactoryAndAvailable(IType ctorParamType, IClass[] superTypes)
        {
            return ctorParamType.GetInterfaceType()?.ShortName == QueryExecutroFactoryInterfaceName
                   && superTypes.Any(x => x.Properties.Any(y => y.ShortName == queryExecutorFactoryFieldName)
                                          || x.Fields.Any(y => y.ShortName == queryExecutorFactoryFieldName));
        }

        public override string Text => "Create mocked class";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            ctor = error?.Reference?.Resolve().DeclaredElement as IConstructor;
            if (ctor == null)
            {
                return false;
            }

            var ctorTreeNode = error?.Reference.GetTreeNode();
            classDeclaration = ctorTreeNode.FindParent<IClassDeclaration>();
            if (classDeclaration == null)
            {
                return false;
            }

            superTypes = classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes).ToArray();
            return superTypes.Any(x => x.GetClassType()?.Methods.Any(y => y.ShortName == "NewMock") ?? false);
        }
    }
}