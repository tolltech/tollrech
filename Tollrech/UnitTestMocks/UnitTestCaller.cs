using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Conversions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Extension;

namespace Tollrech.UnitTestMocks
{
    [QuickFix]
    public class UnitTestCaller : QuickFixBase
    {
        private readonly IncorrectArgumentNumberError error;
        private ITypeConversionRule cSharpTypeConversionRule;

        private IParametersOwner callMethod;
        private IClassDeclaration classDeclaration;
        private IDeclaredType[] superTypes;
        private ICSharpFile file;


        public UnitTestCaller(IncorrectArgumentNumberError error)
        {
            this.error = error;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var callTreeNode = error?.Reference.GetTreeNode();
            if (callTreeNode == null)
            {
                throw new ArgumentNullException();
            }

            if (callMethod == null
                || !(callTreeNode is IReferenceExpression callExpression)
                || !(callTreeNode.Parent is IInvocationExpression ctorArgumentOwner))
            {
                return null;
            }

            var callParams = callMethod.Parameters;
            if (callParams.Count == 0)
            {
                return null;
            }

            var psiModule = error.Reference.GetAccessContext().GetPsiModule();
            cSharpTypeConversionRule = psiModule.GetTypeConversionRule();
            var factory = CSharpElementFactory.GetInstance(psiModule);

            var methodDeclaration = callTreeNode.FindParent<IMethodDeclaration>();

            var existedArguments = ctorArgumentOwner.AllArguments(false)
                .Select(x => new ArgumentInfo
                {
                    Type = x.GetExpressionType()?.ToIType(),
                    Expression = (x as ICSharpArgument)?.Value,
                    IsCSharpArgument = x is ICSharpArgument
                })
                .ToArray();

            var mockInfos = GenerateNewMockInfos(callParams, existedArguments, factory);
            AddMocksToClassDeclaration(methodDeclaration, ctorArgumentOwner, mockInfos);

            var argExpressions = GetCtorArgumentExpressions(existedArguments, callParams);
            var argumentsPattern = string.Join(", ", Enumerable.Range(2, argExpressions.Length).Select(x => $"${x}"));
            var newExpression = factory.CreateExpression($"$0.$1({argumentsPattern});", new[] { callExpression.FirstChild, callExpression.LastChild }.Concat(argExpressions).ToArray());

            ctorArgumentOwner.ReplaceBy(newExpression);

            AddUsings(mockInfos, factory);

            return null;
        }

        private void AddUsings(MockInfo[] mockInfos, CSharpElementFactory factory)
        {
            var usingSymbolDirectives = file.ImportsEnumerable.OfType<IUsingSymbolDirective>().ToArray();
            var namespaces = mockInfos.Select(x => x.Type.GetScalarType()?.GetClrName().GetNamespaceName()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
            foreach (var namespaceName in namespaces)
            {
                var taskUsing = factory.CreateUsingDirective(namespaceName);
                if (usingSymbolDirectives.All(i => i.ImportedSymbolName.QualifiedName != namespaceName))
                {
                    file.AddImport(taskUsing, true);
                }
            }
        }

        private object[] GetCtorArgumentExpressions(ArgumentInfo[] existedArguments, IList<IParameter> ctorParams)
        {
            var argExpressions = new List<object>();
            var existedArgumentsByType = existedArguments.Where(x => x.Expression != null).GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());

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

                if (existedArgumentsByType.TryGetValue(argumentType, out var args) && args.Count > 0)
                {
                    var argument = existedArgumentsByType[argumentType].Pop(x => true);
                    argExpressions.Add(argument.Expression);
                }
                else
                {
                    argExpressions.Add(ctorParam.ShortName);
                }
            }
            return argExpressions.ToArray();
        }

        private static void AddMocksToClassDeclaration(IMethodDeclaration methodDeclaration, ICSharpExpression callExpression, MockInfo[] mockInfos)
        {
            var callStatement = methodDeclaration.Body.Statements
               .FirstOrDefault(x =>
               {
                   var expression = (x as IExpressionStatement)?.Expression;
                   if (expression != null && (expression == callExpression || expression == callExpression.Parent || (expression as IAssignmentExpression)?.Source == callExpression))
                   {
                       return true;
                   }

                   var declarationStatement = (x as IDeclarationStatement);
                   if (declarationStatement?.VariableDeclarations.Any(varDeclaration => (varDeclaration.Initial as IExpressionInitializer)?.Value == callExpression) ?? false)
                   {
                       return true;
                   }

                   return false;
               });

            foreach (var mockInfo in mockInfos)
            {
                var elementHasAssigned = methodDeclaration.Body.Statements.Any(x =>
                {
                    var assignmentOperands = ((x as IExpressionStatement)?.Expression as IAssignmentExpression)?.OperatorOperands;
                    return assignmentOperands != null && assignmentOperands.Any(operand => (operand as IReferenceExpression)?.NameIdentifier.Name == mockInfo.Name);
                });

                if (!elementHasAssigned)
                {
                    methodDeclaration.Body.AddStatementBefore(mockInfo.Statement, callStatement);
                }
            }
        }

        private MockInfo[] GenerateNewMockInfos(IList<IParameter> callParams, ArgumentInfo[] existedArguments, CSharpElementFactory factory)
        {
            var existedArgumentsList = existedArguments.ToList();
            var mockInfos = new List<MockInfo>();
            foreach (var callParam in callParams)
            {
                var isArray = callParam.Type is IArrayType;
                var callParamType = callParam.Type;

                var existArgument = existedArgumentsList.Pop(x => x.IsCSharpArgument && x.Type.IsImplicitlyConvertibleTo(callParam.Type, cSharpTypeConversionRule));
                if (existArgument != null)
                {
                    continue;
                }

                var ctorParamName = callParam.ShortName;
                if (isArray)
                {
                    var scalarType = callParamType.GetScalarType();
                    var singleName = GetSingleName(ctorParamName);

                    var arrayParamNames = Enumerable.Range(1, 2).Select(x => $"{singleName}{x}").ToArray();

                    var existedParamNames = new HashSet<string>(mockInfos.Select(x => x.Name));
                    while (true)
                    {
                        if (arrayParamNames.Any(x => existedParamNames.Contains(x)))
                        {
                            for (var i = 0; i < arrayParamNames.Length; ++i)
                            {
                                arrayParamNames[i] = $"{arrayParamNames[i]}{i + 1}";
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (var arrayParamName in arrayParamNames)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var expression = factory.CreateExpression("$0;", GetParamValue(scalarType.ToIType(), arrayParamName));
                        mockInfos.Add(new MockInfo
                        {
                            Statement = factory.CreateStatement("var $0 = $1;", arrayParamName, expression),
                            Type = ((IArrayType)callParam.Type).ElementType,
                            Name = arrayParamName
                        });
                    }

                    mockInfos.Add(new MockInfo
                    {
                        Statement = factory.CreateStatement("var $0 = $1;", callParam.ShortName, factory.CreateExpression($"new[] {{ {string.Join(", ", arrayParamNames)} }}")),
                        Type = callParam.Type,
                        Name = callParam.ShortName
                    });
                }
                else
                {
                    mockInfos.Add(new MockInfo
                    {
                        Statement = factory.CreateStatement("var $0 = $1;", callParam.ShortName, factory.CreateExpression("$0;", GetParamValue(callParam.Type, callParam.ShortName))),
                        Type = callParam.Type,
                        Name = callParam.ShortName
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

        private int intValue = 42;
        private long longValue = 42L;
        private decimal decimalValue = 42m;
        private double doubleValue = 42;
        private DateTime dateTimeValue = new DateTime(2010, 10, 10);

        private string GetParamValue(IType scalarType, string paramName)
        {
            if (scalarType.IsNullable())
            {
                scalarType = scalarType.GetNullableUnderlyingType();
            }

            if (scalarType.IsInt())
            {
                return $"{intValue--}";
            }

            if (scalarType.IsDecimal())
            {
                return $"{decimalValue--}m";
            }

            if (scalarType.IsLong())
            {
                return $"{longValue--}L";
            }

            if (scalarType.IsShort())
            {
                return $"{intValue--}";
            }

            if (scalarType.IsDouble())
            {
                return $"{doubleValue--}";
            }

            if (scalarType.IsString())
            {
                return $"\"{paramName}\"";
            }

            if (scalarType.IsDateTime())
            {
                var d = dateTimeValue;
                dateTimeValue = dateTimeValue.AddMonths(1).AddDays(1).AddYears(1);
                return $"new DateTime({d.Year}, {d.Month}, {d.Day})";
            }

            if (scalarType.IsGuid())
            {
                return $"Guid.NewGuid()";
            }

            if (scalarType.IsBool())
            {
                return "true";
            }

            var classType = scalarType.GetClassType();
            if (classType != null)
            {
                return $"new {classType.ShortName}()";
            }

            var structType = scalarType.GetStructType();
            if (structType != null)
            {
                return $"new {structType.ShortName}()";
            }

            return "TODO";
        }

        public override string Text => "Create dummy call";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            callMethod = error?.Reference?.Resolve().DeclaredElement as IParametersOwner;
            if (callMethod == null)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            var callTreeNode = error?.Reference.GetTreeNode();
            classDeclaration = callTreeNode.FindParent<IClassDeclaration>();
            if (classDeclaration == null)
            {
                return false;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            file = callTreeNode.GetContainingFile() as ICSharpFile;
            if (file == null)
            {
                return false;
            }

            superTypes = classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes).ToArray();
            return superTypes.Any(x => x.GetClassType()?.Methods.Any(y => y.ShortName == "NewMock") ?? false);
        }
    }
}