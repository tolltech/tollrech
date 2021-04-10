using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Tollrech.Common
{
	public static class ContextActionDataProviderExtensions
    {
	    private static readonly ConcurrentDictionary<(string, string), IDeclaredType> cachedTypes = new ConcurrentDictionary<(string, string), IDeclaredType>();

	    [NotNull]
	    public static IDeclaredType GetType([NotNull] this ICSharpContextActionDataProvider provider, [NotNull] string fullTypeName)
	    {
		    return TypeFactory.CreateTypeByCLRName(new ClrTypeName(fullTypeName), NullableAnnotation.Unknown, provider.PsiModule);
	    }

	    [CanBeNull]
	    public static IAttribute CreateAttribute([NotNull] this ICSharpContextActionDataProvider provider, [NotNull] string attributeFullTypeName)
	    {
		    if (string.IsNullOrWhiteSpace(attributeFullTypeName))
		    {
			    throw new ArgumentException("Value cannot be null or whitespace.", nameof(attributeFullTypeName));
		    }

		    var attributeType = provider.GetType(attributeFullTypeName);
		    var attributeTypeElement = attributeType.GetTypeElement();

		    if (attributeTypeElement == null)
		    {
			    return null;
		    }

		    return provider.ElementFactory.CreateAttribute(attributeTypeElement);
	    }

    }
}
