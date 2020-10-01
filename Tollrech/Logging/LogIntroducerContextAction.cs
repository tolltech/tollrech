using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.CompleteStatement;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl.Tree;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.Logging
{
    [ContextAction(Name = "LogIntroducer", Description = "Introduce logger", Group = "C#", Disabled = false, Priority = 1)]
    public class LogIntroducerContextAction : ContextActionBase
    {
        private readonly CSharpElementFactory factory;
        private readonly IClassDeclaration classDeclaration;
        private readonly ICSharpLiteralExpression literalExpression;
        private readonly ICSharpFile file;

        public LogIntroducerContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            literalExpression = provider.GetSelectedElement<ICSharpLiteralExpression>();
            file = classDeclaration?.GetContainingFile() as ICSharpFile;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            file.AddUsing("log4net", factory);

            var log4netType = CreateIType("log4net.ILog");
            var logField = log4netType != null
                ? classDeclaration.FieldDeclarations.FirstOrDefault(x => x.Type.IsSubtypeOf(log4netType))
                : classDeclaration.FieldDeclarations.FirstOrDefault(x => x.Type.GetInterfaceType()?.ShortName == "ILog");

            if (log4netType == null && logField == null)
            {
                return null;
            }

            if (logField == null)
            {
                logField = CreateFieldDeclaration(log4netType);

                AddKeywords(logField, CSharpTokenType.READONLY_KEYWORD, CSharpTokenType.STATIC_KEYWORD);
                AddInitializer(logField, $"LogManager.GetLogger(typeof({classDeclaration.DeclaredName}))");

                classDeclaration.AddClassMemberDeclaration(logField);
            }

            literalExpression.ReplaceBy(factory.CreateExpression($"{logField.NameIdentifier.Name}.Info($0)", literalExpression));

            return null;
        }

        private void AddInitializer(IFieldDeclaration fieldDeclaration, string initializerExpression)
        {
            var logCreateExpression = factory.CreateExpression(initializerExpression);
            var logInitializer = factory.CreateExpressionInitializer(logCreateExpression);
            fieldDeclaration.SetInitial(logInitializer);
        }

        private static void AddKeywords(IFieldDeclaration fieldDeclaration, params TokenNodeType[] keywords)
        {
            fieldDeclaration.SetModifiersList((IModifiersList)ElementType.MODIFIERS_LIST.Create());

            foreach (var keyword in keywords)
            {
                fieldDeclaration.ModifiersList.AddModifier((ITokenNode)keyword.CreateLeafElement());
            }
        }

        private IFieldDeclaration CreateFieldDeclaration(IType fieldType)
        {
            var fieldDeclaration = factory.CreateFieldDeclaration(fieldType, "log");
            return fieldDeclaration;
        }

        private IType CreateIType(string typeName)
        {
            var type = CSharpTypeFactory.CreateType(typeName, classDeclaration);

            return !type.IsResolved ? null : type;
        }

        public override string Text => "Log this";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return classDeclaration != null && literalExpression != null && file != null
                   && literalExpression.HasParent<ICSharpArgument>() == null
                   && literalExpression.HasParent<IVariableInitializer>() == null;
        }
    }
}