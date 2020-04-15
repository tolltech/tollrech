using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.EFClass
{
    [ContextAction(Name = "SqlRepositoryGenerator", Description = "Generate Sql repository for class-entity", Group = "C#", Disabled = false, Priority = 1)]
    public class SqlRepositoryGeneratorContextAction : ContextActionBase
    {
        private readonly IClassDeclaration classDeclaration;
        private readonly CSharpElementFactory factory;
        private string className;

        public SqlRepositoryGeneratorContextAction([NotNull] ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            classDeclaration = provider.GetSelectedElement<IClassDeclaration>();
            ;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var entityName = className.Substring(0, className.Length - 3);

            var repositoryInterfaceName = $"I{entityName}Repository";

            var handlerInterfaceName = $"I{entityName}Handler";
            var interfaceHandlerDeclaration = CreateInterfaceHandlerDeclaration(entityName, handlerInterfaceName);
            AddDeclaration(classDeclaration, interfaceHandlerDeclaration);

            var classHandlerDeclaration = CreateClassHandlerDeclaration(entityName, handlerInterfaceName);
            AddDeclaration(classDeclaration, classHandlerDeclaration);

            var interfaceRepositoryDeclaration = CreateInterfaceRepositoryDeclaration(entityName, repositoryInterfaceName);
            AddDeclaration(classDeclaration, interfaceRepositoryDeclaration);

            var classRepositoryDeclaration = CreateClassRepositoryDeclaration(entityName, repositoryInterfaceName, handlerInterfaceName);
            AddDeclaration(classDeclaration, classRepositoryDeclaration);

            return null;
        }

        [NotNull]
        private IClassLikeDeclaration CreateClassHandlerDeclaration([NotNull] string entityName, [NotNull] string handlerInterfaceName)
        {
            var classHandlerName = $"{entityName}Handler";
            var classHandlerDeclaration = (IClassLikeDeclaration)factory.CreateTypeMemberDeclaration($"public class $0 : EntityHandlerBase<{className}>, $1 {{" +
                                                                                                     $"    public $0(IDataContextProvider dataContextProvider) : base(dataContextProvider){{}}" +
                                                                                                     $"}}", classHandlerName, handlerInterfaceName);
            return classHandlerDeclaration;
        }

        [NotNull]
        private IClassLikeDeclaration CreateInterfaceHandlerDeclaration([NotNull] string entityName, [NotNull] string handlerInterfaceName)
        {
            var entityNameManies = entityName.MorphemToManies();
            var interfaceHandlerDeclaration = (IClassLikeDeclaration)factory.CreateTypeMemberDeclaration("public interface $0 {" +
                                                                                                            "    [NotNull]" +
                                                                                                            $"    Task CreateAsync([NotNull] params {className}[] {entityNameManies.MakeFirsCharLowercase()});" +
                                                                                                            "}", handlerInterfaceName);
            return interfaceHandlerDeclaration;
        }

        [NotNull]
        private IClassLikeDeclaration CreateClassRepositoryDeclaration([NotNull] string entityName, [NotNull] string repositoryInterfaceName, [NotNull] string handlerInterfaceName)
        {
            var classHandlerName = handlerInterfaceName.Substring(1, handlerInterfaceName.Length - 1).MakeFirsCharLowercase();
            var entityNameManies = entityName.MorphemToManies();
            var classRepositoryName = $"{entityName}Repository";
            var classRepositoryDeclaration = (IClassLikeDeclaration)factory.CreateTypeMemberDeclaration("[SqlScope]" +
                                                                                                        "public class $0 : $1 {" +
                                                                                                        "" +
                                                                                                        "    private readonly $2 $3;" +
                                                                                                        "" +
                                                                                                        "    public $0($2 $3){" +
                                                                                                        "    this.$3 = $3;" +
                                                                                                        "    }" +
                                                                                                        "    [SqlScope(true)]" +
                                                                                                        $"    public Task CreateAsync(params {className}[] {entityNameManies.MakeFirsCharLowercase()}){{" +
                                                                                                        $"        return $3.CreateAsync({entityNameManies.MakeFirsCharLowercase()});" +
                                                                                                        $"}}" +
                                                                                                        "}", classRepositoryName, repositoryInterfaceName, handlerInterfaceName, classHandlerName);
            return classRepositoryDeclaration;
        }

        [NotNull]
        private IClassLikeDeclaration CreateInterfaceRepositoryDeclaration([NotNull] string entityName, [NotNull] string repositoryInterfaceName)
        {
            var entityNameManies = entityName.MorphemToManies();
            var interfaceRepositoryDeclaration = (IClassLikeDeclaration)factory.CreateTypeMemberDeclaration("public interface $0 {" +
                                                                                                            "    [NotNull]" +
                                                                                                            $"    Task CreateAsync([NotNull] params {className}[] {entityNameManies.MakeFirsCharLowercase()});" +
                                                                                                            "}", repositoryInterfaceName);
            return interfaceRepositoryDeclaration;
        }

        private static void AddDeclaration([NotNull] IClassLikeDeclaration anchor, [NotNull] IClassLikeDeclaration declaration)
        {
            var holderDeclaration = (ICSharpTypeAndNamespaceHolderDeclaration)CSharpNamespaceDeclarationNavigator.GetByTypeDeclaration(anchor)
                                    ?? CSharpFileNavigator.GetByTypeDeclaration(anchor);
            if (holderDeclaration == null)
            {
                anchor.AddClassMemberDeclaration(declaration);
            }
            else
            {
                holderDeclaration.AddTypeDeclarationAfter(declaration, anchor);
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            className = classDeclaration?.DeclaredName;
            return !className.IsNullOrWhitespace() && className.EndsWith("Dbo") && className.Length > 3;
        }

        public override string Text => "Generate repository classes";
    }
}