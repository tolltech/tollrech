﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using Tollrech.EFClass;

namespace Tollrech.Common
{
    public static class SyntaxExtensions
    {
        public static PropertyInfo GetPropertyInfo([NotNull] this IPropertyDeclaration propertyDeclaration)
        {
            return new PropertyInfo
                   {
                       ColumnName = propertyDeclaration.Attributes.FindAttribute(Constants.Column)?.Arguments.FirstOrDefault().GetLiteralText() ?? "TODOColumnName",
                       Required = propertyDeclaration.Attributes.HasAttribute(Constants.Required),
                       Key = propertyDeclaration.Attributes.HasAttribute(Constants.Key),
                       MaxLength = propertyDeclaration.Attributes.FindAttribute(Constants.MaxLength)?.Arguments.FirstOrDefault().GetLiteralText(),
                       Precision1 = propertyDeclaration.Attributes.FindAttribute(Constants.DecimalPrecision)?.Arguments.FirstOrDefault().GetLiteralText(),
                       Precision2 = propertyDeclaration.Attributes.FindAttribute(Constants.DecimalPrecision)?.Arguments.LastOrDefault().GetLiteralText(),
                       IsTimestamp = propertyDeclaration.Attributes.FindAttribute(Constants.TimestampAttribute) != null,
                       Declaration = propertyDeclaration
                   };
        }

        public static void AddXmlComment(this ITypeMemberDeclaration declaration, string text, CSharpElementFactory factory)
        {
            var docCommentBlockOwnerNode = XmlDocTemplateUtil.FindDocCommentOwner(declaration);

            if (docCommentBlockOwnerNode == null)
            {
                return;
            }

            var comment = factory.CreateDocCommentBlock(text);
            docCommentBlockOwnerNode.SetDocCommentBlock(comment);
        }

        [CanBeNull]
        public static IAttribute FindAttribute([NotNull] this IEnumerable<IAttribute> src, string name)
        {
            return src.FirstOrDefault(x => x.Name.NameIdentifier.Name == name);
        }

        public static bool HasAttribute([NotNull] this IEnumerable<IAttribute> src, string name)
        {
            return src.Any(x => x.Name.NameIdentifier.Name == name);
        }

        [CanBeNull]
        public static string GetLiteralText([CanBeNull] this ICSharpArgument src)
        {
            return (src?.Expression as ICSharpLiteralExpression)?.Literal.GetText().Trim('"');
        }

        public static bool HasGetSet([NotNull] this IPropertyDeclaration src)
        {
            if (src.AccessorDeclarations.All(x => x.Kind != AccessorKind.GETTER))
            {
                return false;
            }

            if (src.AccessorDeclarations.All(x => x.Kind != AccessorKind.SETTER))
            {
                return false;
            }

            return true;
        }

        [NotNull]
        public static IEnumerable<IDeclaredType> GetAllSuperTypes([CanBeNull] this IClassDeclaration classDeclaration)
        {
            if (classDeclaration == null)
            {
                return Enumerable.Empty<IDeclaredType>();
            }

            return classDeclaration.SuperTypes.SelectMany(x => x.GetAllSuperTypes()).Concat(classDeclaration.SuperTypes);
        }

        public static IEnumerable<ITreeNode> GetAllDescendants([CanBeNull] this ITreeNode root, [CanBeNull] StringBuilder sb = null, int level = 0)
        {
            if (root == null)
            {
                yield break;
            }
            sb?.AppendLine("{");
            if (root.Descendants().Any())
            {
                sb?.AppendLine("\"Childs\":[");
            }

            foreach (var descendant in root.Descendants())
            {
                var valueStr = descendant.ToString().Replace("\r", "").Replace("\n", "").Trim();
                sb?.AppendLine("{");
                //sb?.AppendLine($"{string.Join("\t", Enumerable.Range(0, level).Select(x => string.Empty))}{valueStr}\t{descendant.GetType().Name}");
                sb?.AppendLine($"\"Type\":\"{descendant.GetType().Name}\",");
                sb?.AppendLine($"\"Value\":\"{valueStr.Replace("\"","'")}\"");

                yield return descendant;

                sb?.AppendLine("},");
                foreach (var child in descendant.GetAllDescendants(sb, level + 1))
                {
                    yield return child;
                }
            }

            if (root.Descendants().Any())
            {
                sb?.AppendLine("]");
            }

            sb?.AppendLine("},");
        }
    }

    public class DummyHelper
    {
        private int intValue = 42;
        private long longValue = 42L;
        private decimal decimalValue = 42m;
        private double doubleValue = 42;
        private DateTime dateTimeValue = new DateTime(2010, 10, 10);

        [NotNull]
        public string GetParamValue(IType scalarType, [CanBeNull] string paramName = null)
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
                return $"\"{paramName ?? "TODO"}\"";
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
    }
}