using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "MsSqlMapGenerate", Description = "Generate MsSql map for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlMapGeneratorMsContextAction : SqlMapGeneratorContextActionBase
    {
        public SqlMapGeneratorMsContextAction(ICSharpContextActionDataProvider provider) : base(provider, "ColumnTypeNames", GetDbColumnTypeName)
        {

        }

        [CanBeNull]
        public static string GetDbColumnTypeName(IType scalarType)
        {
            if (scalarType.IsInt() || scalarType.IsEnumType())
            {
                return Constants.Int;
            }

            if (scalarType.IsGuid())
            {
                return Constants.UniqueIdentifier;
            }

            if (scalarType.IsString())
            {
                return Constants.NVarChar;
            }

            if (scalarType.IsBool())
            {
                return Constants.Bit;
            }

            if (scalarType.IsDateTime())
            {
                return Constants.DateTime2;
            }

            if (scalarType.IsLong())
            {
                return Constants.BigInt;
            }

            if (scalarType.IsDecimal())
            {
                return Constants.Decimal;
            }

            return null;
        }

        public override string Text => "Add ms data annotation mapping";
    }
}