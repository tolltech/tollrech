﻿using System;

namespace Tollrech.Tests.Test.Data.SqlMapGeneratorTests
{
    public class SqlEntity{caret}
    {
        [Column("Int", TypeName = ColumnTypeNames.Int)]
        [ConcurrencyCheck]
        [Required]
        public int Int { get; set; }
        public decimal Decimal { get; set; }
    }

    public enum MyEnum
    {
    }
}

#region Infra
namespace System.ComponentModel.DataAnnotations.Schema
{
    public class TableAttribute : Attribute
    {
        private readonly string name;

        public TableAttribute(string name)
        {
            this.name = name;
        }
    }

    public class ColumnAttribute : Attribute
    {
        private readonly string name;

        public ColumnAttribute(string name)
        {
            this.name = name;
        }

        public string TypeName { get; set; }
    }
}

namespace System.ComponentModel.DataAnnotations
{
    public class KeyAttribute : Attribute
    {

    }

    public class ConcurrencyCheckAttribute : Attribute
    {

    }

    public class RequiredAttribute : Attribute
    {
        public bool AllowEmptyStrings { get; set; }
    }

    public class MaxLengthAttribute : Attribute
    {
        private readonly int i;

        public MaxLengthAttribute(int i)
        {
            this.i = i;
        }
    }
}

namespace SKBKontur.Billy.Core.Common.Quering
{
    public static class ColumnTypeNames
    {
        public const string UniqueIdentifier = "uniqueidentifier";
        public const string Int = "int";
        public const string NVarChar = "nvarchar";
        public const string Bit = "bit";
        public const string DateTime2 = "datetime2";
        public const string BigInt = "bigint";
        public const string SmallInt = "smallint";
        public const string Date = "date";
        public const string Decimal = "decimal";
    }
}

namespace SKBKontur.Billy.Core.Common.Quering.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DecimalPrecisionAttribute : Attribute
    {
        public DecimalPrecisionAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public byte Precision { get; set; }
        public byte Scale { get; set; }
    }
}

#endregion