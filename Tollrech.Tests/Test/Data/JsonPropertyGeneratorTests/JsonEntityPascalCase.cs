using System;

namespace Tollrech.Tests.Test.Data.SqlMapGeneratorTests
{
    public class JsonEntityPascalCase{caret:Add:JsonProperty:attributes:PascalCase}
    {
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public string String { get; set; }
        public decimal? NullableDecimal { get; set; }
        public MyEnum? MyEnum { get; set; }
        public DateTime DateTime { get; set; }
        public Guid Guid { get; set; }
        public bool Bool { get; set; }
        public long Long { get; set; }
        public string snake_case { get; set; }
        public string camelCase { get; set; }
        public long LongGet { get; }
        public long LongSet { set; }
        public long LongField;
        public long LongFunction()
        {

        }
        public long LongFunction() => 42L;
    }

    public enum MyEnum
    {
    }
}

#region Infra
namespace Newtonsoft.Json
{
    public class JsonPropertyAttribute : Attribute
    {
        public JsonPropertyAttribute()
        {
        }

        public JsonPropertyAttribute(string name)
        {
        }
    }
}

#endregion
