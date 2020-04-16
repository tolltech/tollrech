using System;

namespace Tollrech.Tests.Test.Data.SqlMapGeneratorTests
{
    [Table("InvoiceTransactions")]
    public class InvoiceTransactionDbo{caret}
    {
        [Column("Id", TypeName = ColumnTypeNames.UniqueIdentifier), Key, ConcurrencyCheck, Required]
        public Guid Id { get; set; }

        [Column("FromBillId", TypeName = ColumnTypeNames.UniqueIdentifier), ConcurrencyCheck]
        public Guid? FromBillId { get; set; }

        [Column("ToBillId", TypeName = ColumnTypeNames.UniqueIdentifier), ConcurrencyCheck]
        public Guid? ToBillId { get; set; }

        [Column("CashTransactionId", TypeName = ColumnTypeNames.UniqueIdentifier), ConcurrencyCheck, Required]
        public Guid CashTransactionId { get; set; }

        [Column("Amount", TypeName = ColumnTypeNames.Decimal), ConcurrencyCheck, Required]
        public long Amount { get; set; }

        [Column("OperationDate", TypeName = ColumnTypeNames.DateTime2), ConcurrencyCheck, Required]
        public DateTime OperationDate { get; set; }

        [Column("Type", TypeName = ColumnTypeNames.Int), ConcurrencyCheck, Required]
        public InvoiceTransactionType Type { get; set; }

        [Column("Comment", TypeName = ColumnTypeNames.NVarChar), ConcurrencyCheck, Required(AllowEmptyStrings = true)]
        public string Comment { get; set; }

        [Column("Timestamp", TypeName = ColumnTypeNames.BigInt), ConcurrencyCheck, Required]
        public long Timestamp { get; set; }
    }
}