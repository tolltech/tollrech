namespace Tollrech.Tests.Test.Data.SqlMapGeneratorTests
{
    [Table("InvoiceTransactions")]
    public class EmptyEntity{caret}
    {
        public int Field;
        public int PropGetField => 42;
        public int G { get; }
        public int S { set; }
    }
}