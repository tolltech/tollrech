using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using Tollrech.Common;

namespace Tollrech.NetworkProxy
{
    [ContextAction(Name = "NetworkProxySignature", Description = "NetworkProxySignature", Group = "C#", Disabled = false, Priority = 1)]
    public class NetworkProxySignatureContextAction : ContextActionBase
    {
        private readonly CSharpElementFactory factory;
        private readonly IMethodDeclaration methodDeclaration;
        private readonly IInterfaceDeclaration interfaceDeclaration;
        private string interfaceName;

        public NetworkProxySignatureContextAction(ICSharpContextActionDataProvider provider)
        {
            factory = provider.ElementFactory;
            methodDeclaration = provider.GetSelectedElement<IMethodDeclaration>();
            interfaceDeclaration = provider.GetSelectedElement<IInterfaceDeclaration>();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var text = GetNetworkProxySignatureString();
            methodDeclaration.AddXmlComment(text, factory);

            return null;
        }

        [NotNull]
        private string GetNetworkProxySignatureString()
        {
            return "babam";
        }

        public override string Text => "Generate http signature";
        public override bool IsAvailable(IUserDataHolder cache)
        {
            var networkServiceAttribute = interfaceDeclaration?.Attributes.FindAttribute("NetworkService");
            interfaceName = interfaceDeclaration?.NameIdentifier?.Name;

            return networkServiceAttribute != null && interfaceName != null && methodDeclaration != null;
        }
    }
}