﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SKBKontur.Billy.Core.Common.Quering;
using SKBKontur.Billy.Core.Common.Quering.Attributes;

namespace Tollrech.Tests.Test.Data.SqlScriptGeneratorTests
{
    [Table("SqlEntities")]
    public class SqlEntity2
    {
        [Column("Id", TypeName = ColumnTypeNames.UniqueIdentifier), Key, Required, ConcurrencyCheck]
        public Guid Id { get; set; }

        [Column("Amount", TypeName = "decimal"), Required, ConcurrencyCheck, DecimalPrecision(18, 2)]
        public decimal Amount{caret:Generate:sql:script} { get; set; }

        [Column("Number", TypeName = ColumnTypeNames.NVarChar), Required(AllowEmptyStrings = true), ConcurrencyCheck, MaxLength(50)]
        public string Number { get; set; }

        [Column("Type_New", TypeName = "int"), Required, ConcurrencyCheck]
        public MyEnum2 Type { get; set; }

        [Column("KbaIncomingDate", TypeName = "datetime2"), ConcurrencyCheck]
        public DateTime? KbaIncomingDate { get; set; }

        [Column("TimeStamp", TypeName = "bigint"), Required, ConcurrencyCheck]
        public long TimeStamp { get; set; }

        [Column("IsDeleted", TypeName = ColumnTypeNames.Bit), Required, ConcurrencyCheck]
        public bool IsDeleted { get; set; }
    }

    public enum MyEnum2
    {

    }
}
