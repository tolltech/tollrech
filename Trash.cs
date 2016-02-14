using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.LinqTools;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;

namespace Tollrech
{
    public class Trash
    {
        private readonly AccessRightsError _error;
        private readonly IDeclaredElement _declaredElement;
        private PsiLanguageType _languageForPresentation;

        public Trash(AccessRightsError error)
        {
            _error = error;
            _declaredElement = error.Reference.CurrentResolveResult.DeclaredElement;
            _languageForPresentation = error.Reference.GetTreeNode().Language;
        }

        protected Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var accessExpression = _error.Reference.GetTreeNode() as IExpression;
            var replacementNode = accessExpression;
            if (replacementNode == null)
                return null;
            var modifiers = _declaredElement as IModifiersOwner;
            if (modifiers == null)
                return null;
            bool isAssign = replacementNode.Parent is IAssignmentExpression;
            bool needsCasting = !isAssign && !(replacementNode.Parent is IExpressionStatement)
                && !_declaredElement.Type().IsVoid() && !_declaredElement.Type().IsObject();
            if (replacementNode.Parent is IInvocationExpression || replacementNode.Parent is IAssignmentExpression)
            {
                replacementNode = (IExpression)replacementNode.Parent;
            }
            CSharpElementFactory factory = CSharpElementFactory.GetInstance(replacementNode, applyCodeFormatter: true);
            AddSystemReflectionNamespace(factory);

            string flags = "BindingFlags.NonPublic";
            if (modifiers.IsStatic)
            {
                flags += "| BindingFlags.Static";
            }
            else
            {
                flags += "| BindingFlags.Instance";
            }
            flags += "| " + GetInvokeMemberBindingFlag(_declaredElement, isAssign);
            IExpression instanceExpression = modifiers.IsStatic ? factory.CreateExpression("null") : ((IReferenceExpression)accessExpression).QualifierExpression;
            IExpression argsExpression = factory.CreateExpression("null");
            if (isAssign)
            {
                argsExpression = factory.CreateExpression("new object[] { $0 }",
                    ((IAssignmentExpression)replacementNode).Source);
            }
            if (replacementNode is IInvocationExpression)
            {
                var invocationExpression = (IInvocationExpression)replacementNode;
                if (invocationExpression.Arguments.Count != 0)
                {

                //    argsExpression = CreateArrayCreationExpression(
                //        TypeFactory.CreateTypeByCLRName(
                //"System.Object",
                //        accessExpression.GetPsiModule(),
                //        accessExpression.GetResolveContext()), factory);
                    var arrayCreationExpression = argsExpression as IArrayCreationExpression;
                    foreach (var arg in invocationExpression.ArgumentsEnumerable)
                    {
                        var initiallizer = factory.CreateVariableInitializer((ICSharpExpression)arg.Expression);
                        arrayCreationExpression.ArrayInitializer.AddElementInitializerBefore(initiallizer, null);
                    }
                }
            }
            var reflectionExpression = factory.CreateExpression("typeof($0).InvokeMember(\"$1\", $2, null, $3, $4)",
        
                ((IClrDeclaredElement)_declaredElement).GetContainingType(),
                _declaredElement.ShortName,
                flags,
                instanceExpression,
                argsExpression);
            if (needsCasting)
            {
                reflectionExpression = factory.CreateExpression("($0)$1",
            _declaredElement.Type(),
            reflectionExpression);
            }
            replacementNode.ReplaceBy(reflectionExpression);
            return null;
        }

        private IExpression CreateArrayCreationExpression(object createTypeByClrName, CSharpElementFactory factory)
        {
            throw new NotImplementedException();
        }

        private object GetInvokeMemberBindingFlag(IDeclaredElement declaredElement, bool isAssign)
        {
            throw new NotImplementedException();
        }

        private void AddSystemReflectionNamespace(CSharpElementFactory factory)
        {
            var importScope = CSharpReferenceBindingUtil.GetImportScope(_error.Reference);
            var reflectionNamespace = GetReflectionNamespace(factory);
            if (true)//!UsingUtil.CheckAlreadyImported(importScope, reflectionNamespace))
            {
                UsingUtil.AddImportTo(importScope, reflectionNamespace);
            }
        }
private static INamespace GetReflectionNamespace(CSharpElementFactory factory)
        {
            var usingDirective = factory.CreateUsingDirective("System.Reflection");
            var reference = usingDirective.ImportedSymbolName;
            var reflectionNamespace = reference.Reference.Resolve().DeclaredElement as INamespace;
            return reflectionNamespace;
        }
    }
}