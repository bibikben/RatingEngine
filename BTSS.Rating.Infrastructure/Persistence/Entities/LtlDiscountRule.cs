using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("LtlDiscountRules", Schema="rating")]
public sealed class LtlDiscountRule
{
    [Key]
    [Column("LtlDiscountRuleId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LtlDiscountRuleId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("NmfcClass")]
    public int? NmfcClass { get; set; }

    [Column("DiscountPercent")]
    [Precision(18, 6)]
    public decimal DiscountPercent { get; set; }

    [Column("MinChargeOverride")]
    [Precision(18, 6)]
    public decimal? MinChargeOverride { get; set; }

    [Column("ConditionsJson")]
    public string? ConditionsJson { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
