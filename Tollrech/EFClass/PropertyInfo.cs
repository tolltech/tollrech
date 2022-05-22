using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using Tollrech.Common;

namespace Tollrech.EFClass
{
    public struct PropertyInfo
    {
        public IPropertyDeclaration Declaration { get; set; }
        public string ColumnName { get; set; }
        public bool Required { get; set; }
        public bool Key { get; set; }
        public string MaxLength { get; set; }
        public string Precision1 { get; set; }
        public string Precision2 { get; set; }
        public bool IsTimestamp { get; set; }

        private static readonly Dictionary<string, string> mssqlCodedTypes = new Dictionary<string, string>
                                                                        {
                                                                            {Constants.BigInt, "bigint"},
                                                                            {Constants.Bit, "bit"},
                                                                            {Constants.DateTime2, "datetime2"},
                                                                            {Constants.Decimal, "decimal"},
                                                                            {Constants.Int, "int"},
                                                                            {Constants.UniqueIdentifier, "uniqueidentifier"},
                                                                            {Constants.NVarChar, "nvarchar"},
                                                                            {Constants.Date, "date"},
                                                                            {Constants.Image, "image"},
                                                                            {Constants.VarBinary, "varbinary"},
                                                                        };

        //todo: адаптировать
        private static readonly Dictionary<string, string> postgresCodedTypes = new Dictionary<string, string>
                                                                        {
                                                                            {Constants.BigInt, "bigint"},
                                                                            {Constants.Bit, "bit"},
                                                                            {Constants.DateTime2, "datetime2"},
                                                                            {Constants.Decimal, "decimal"},
                                                                            {Constants.Int, "int"},
                                                                            {Constants.UniqueIdentifier, "uuid"},
                                                                            {Constants.NVarChar, "varchar"},
                                                                            {Constants.Date, "timestamp without time zone"},
                                                                            {Constants.Image, "image"},
                                                                            {Constants.VarBinary, "varbinary"},
                                                                        };

        [NotNull]
        public string GetColumnType(DbType dbType = DbType.Ms)
        {
            var typeNameExpression = Declaration.Attributes.FindAttribute(Constants.Column)?.PropertyAssignments.FirstOrDefault(x => x.PropertyNameIdentifier.Name == Constants.TypeName)?.Source;
            if (typeNameExpression is ICSharpLiteralExpression literalExpression)
            {
                return literalExpression.GetText().Trim('"');
            }

            if (typeNameExpression is IReferenceExpression referenceExpression)
            {
                var codedType = referenceExpression.NameIdentifier.Name;
                var codedTypeNames = dbType == DbType.Postgres ? postgresCodedTypes : mssqlCodedTypes;
                return codedTypeNames.TryGetValue(codedType, out var value) ? value : codedType.ToLower();
            }

            if (Declaration.Attributes.FindAttribute(Constants.TimestampAttribute) != null)
            {
                return "rowversion";
            }

            return "TODOColumnType";
        }

    }
}