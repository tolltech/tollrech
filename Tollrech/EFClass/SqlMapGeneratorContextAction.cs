using System;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlMapGenerate", Description = "Generate Sql map for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlMapGeneratorContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider provider;
        private readonly IClassDeclaration classDeclaration;

        public SqlMapGeneratorContextAction(ICSharpContextActionDataProvider provider)
        {
            this.provider = provider;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var factory = provider.ElementFactory;

            provider.PsiModule.Get

            ITypeElement typeElement = provider.PsiServices
                .Caches
                .
                .GetDeclarationsCache(psiModule, true, false)
                .GetTypeElementsByCLRName(new ClrTypeName("System.Runtime.Serialization.DataMemberAttribute"))
                .First();

            foreach (var propertyDeclaration in classDeclaration.PropertyDeclarations)
            {
                factory.CreateAttribute(factory.Cre)

                propertyDeclaration.AddAttributeBefore(new);
            }


            return null;
        }

        public override string Text => "Add EF Sql map";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return classDeclaration != null;
        }
    }
}