using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("LtlBaseRates", Schema="rating")]
public sealed class LtlBaseRate
{
    [Key]
    [Column("LtlBaseRateId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LtlBaseRateId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("OriginZoneId")]
    public int OriginZoneId { get; set; }

    [Column("DestZoneId")]
    public int DestZoneId { get; set; }

    [Column("NmfcClass")]
    public int NmfcClass { get; set; }

    [Column("WeightMinLbs")]
    public int WeightMinLbs { get; set; }

    [Column("WeightMaxLbs")]
    public int WeightMaxLbs { get; set; }

    [Column("RatePerCwt")]
    [Precision(18, 6)]
    public decimal RatePerCwt { get; set; }

    [Column("MinimumCharge")]
    [Precision(18, 6)]
    public decimal? MinimumCharge { get; set; }

    [Column("DeficitRuleJson")]
    public string? DeficitRuleJson { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
