using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.CompleteStatement;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl.Tree;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
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

            var fieldDeclaration = CreateFieldDeclaration("log4net.ILog");
            if (fieldDeclaration == null)
            {
                return null;
            }

            AddKeywords(fieldDeclaration, CSharpTokenType.READONLY_KEYWORD, CSharpTokenType.STATIC_KEYWORD);
            AddInitializer(fieldDeclaration, $"LogManager.GetLogger(typeof({classDeclaration.DeclaredName}))");

            classDeclaration.AddClassMemberDeclaration(fieldDeclaration);

            literalExpression.ReplaceBy(factory.CreateExpression("log.Info($0)", literalExpression));

            return null;
        }

        private void AddInitializer([NotNull] IFieldDeclaration fieldDeclaration, [NotNull] string initializerExpression)
        {
            var logCreateExpression = factory.CreateExpression(initializerExpression);
            var logInitializer = factory.CreateExpressionInitializer(logCreateExpression);
            fieldDeclaration.SetInitial(logInitializer);
        }

        private static void AddKeywords([NotNull] IFieldDeclaration fieldDeclaration, [NotNull] params TokenNodeType[] keywords)
        {
            fieldDeclaration.SetModifiersList((IModifiersList)ElementType.MODIFIERS_LIST.Create());

            foreach (var keyword in keywords)
            {
                fieldDeclaration.ModifiersList.AddModifier((ITokenNode)keyword.CreateLeafElement());
            }
        }

        [CanBeNull]
        private IFieldDeclaration CreateFieldDeclaration([NotNull] string fieldType)
        {
            var lognetType = CSharpTypeFactory.CreateType(fieldType, classDeclaration);

            if (!lognetType.IsResolved)
            {
                return null;
            }

            var fieldDeclaration = factory.CreateFieldDeclaration(lognetType, "log");
            return fieldDeclaration;
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