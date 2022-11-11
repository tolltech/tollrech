﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SKBKontur.Billy.Core.Common.Quering;
using SKBKontur.Billy.Core.Common.Quering.Attributes;

namespace Tollrech.Tests.Test.Data.SqlScriptGeneratorTests
{
    [Table("SqlEntities")]
    public class SqlEntity2{caret:Generate:psql:script}
    {
        [Column("Id", TypeName = ColumnTypeNames.UniqueIdentifier), Key, Required, ConcurrencyCheck]
        public Guid Id { get; set; }

        [Column("Amount", TypeName = ColumnTypeNames.Decimal), Required, ConcurrencyCheck, DecimalPrecision(18, 2)]
        public decimal Amount { get; set; }
    
        [Column("Amount2", TypeName = PostgreSqlColumnTypeNames.numeric), Required, ConcurrencyCheck, DecimalPrecision(18, 2)]
        public decimal Amount2 { get; set; }

        [Column("Number", TypeName = ColumnTypeNames.NVarChar), Required(AllowEmptyStrings = true), ConcurrencyCheck, MaxLength(50)]
        public string Number { get; set; }

        [Column("Number2", TypeName = ColumnTypeNames.NVarChar), Required(AllowEmptyStrings = true), ConcurrencyCheck]
        public string Number2 { get; set; }

        [Column("Type_New", TypeName = "int"), Required, ConcurrencyCheck]
        public MyEnum2 Type { get; set; }

        [Column("KbaIncomingDate", TypeName = "datetime2"), ConcurrencyCheck]
        public DateTime? KbaIncomingDate { get; set; }

        [Column("TimeStamp", TypeName = PostgreSqlColumnTypeNames.int8), Required, ConcurrencyCheck]
        public long TimeStamp { get; set; }

        [Column("IsDeleted", TypeName = ColumnTypeNames.Bit), Required, ConcurrencyCheck]
        public bool IsDeleted { get; set; }

        [Column("Timestamp2"), Timestamp, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] Timestamp2 { get; set; }
    }

    public enum MyEnum2
    {

    }
}
