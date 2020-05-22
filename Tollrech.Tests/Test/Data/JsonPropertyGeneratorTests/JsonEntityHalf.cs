using System;

namespace Tollrech.Tests.Test.Data.SqlMapGeneratorTests
{
    public class JsonEntity{caret:Add:JsonProperty:attributes}
    {
        [JsonProperty("Int")] public int Int { get; set; }
        public decimal Decimal { get; set; }
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
