using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace Tollrech.Common
{
    public static class SyntaxExtensions
    {
        [CanBeNull]
        public static IAttribute FindAttribute([NotNull] this IEnumerable<IAttribute> src, string name)
        {
            return src.FirstOrDefault(x => x.Name.NameIdentifier.Name == name);
        }

        public static bool HasAttriobute([NotNull] this IEnumerable<IAttribute> src, string name)
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