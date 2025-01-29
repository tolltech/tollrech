using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SKBKontur.Billy.Core.Common.Quering;
using SKBKontur.Billy.Core.Common.Quering.Attributes;

namespace Tollrech.Tests.Test.Data.SqlScriptGeneratorTests
{
    [Table("SqlEntities")]
    public class SqlEntity2{caret:Generate:psql:script}
    {
    [Column("id", TypeName = "uuid"), Key, Required]
    public Guid Id { get; set; }

    [Column("food_id", TypeName = "varchar"), Required]
    public string FoodId { get; set; }

    [Column("name", TypeName = "varchar"), Required]
    public string Name { get; set; }

    [Column("chat_id", TypeName = "bigint"), Required]
    public long ChatId { get; set; }

    [Column("user_id", TypeName = "bigint"), Required]
    public long UserId { get; set; }

    [Column("message_date"), Required]
    public DateTimeOffset MessageDate { get; set; }

    [Column("kcal", TypeName = "int"), Required]
    public int Kcal { get; set; }

    [Column("protein", TypeName = "int"), Required]
    public int Protein { get; set; }

    [Column("fat", TypeName = "int"), Required]
    public int Fat { get; set; }

    [Column("carbohydrate", TypeName = "int"), Required]
    public int Carbohydrate { get; set; }
    }

    public enum MyEnum2
    {

    }
}
