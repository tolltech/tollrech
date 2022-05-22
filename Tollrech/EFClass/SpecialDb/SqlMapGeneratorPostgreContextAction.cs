using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using Tollrech.Common;
using Tollrech.EFClass.Base;

namespace Tollrech.EFClass.SpecialDb
{
    [ContextAction(Name = "MsSqlMapGenerate", Description = "Generate PSql map for class-entity", Group = "C#", Disabled = true, Priority = 1)]
    public class SqlMapGeneratorPostgreContextAction : SqlMapGeneratorContextActionBase
    {
        public SqlMapGeneratorPostgreContextAction(ICSharpContextActionDataProvider provider) : base(provider, "PostgreSqlColumnTypeNames", GetDbColumnTypeName, InflectorExtensions.Underscore)
        {

        }

        [CanBeNull]
        private static string GetDbColumnTypeName(IType scalarType)
        {
            if (scalarType.IsInt() || scalarType.IsEnumType())
            {
                return Constants.int4;
            }

            if (scalarType.IsGuid())
            {
                return Constants.uuid;
            }

            if (scalarType.IsString())
            {
                return Constants.varchar;
            }

            if (scalarType.IsBool())
            {
                return Constants.boolean;
            }

            if (scalarType.IsDateTime())
            {
                return Constants.timestamp;
            }

            if (scalarType.IsLong())
            {
                return Constants.int8;
            }

            if (scalarType.IsDecimal())
            {
                return Constants.numeric;
            }

            return null;
        }

        public override string Text => "Add psql data annotation mapping";
    }
}