using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("LclRates", Schema="rating")]
public sealed class LclRate
{
    [Key]
    [Column("LclRateId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LclRateId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("OriginPort")]
    [MaxLength(10)]
    public string OriginPort { get; set; }

    [Column("DestPort")]
    [MaxLength(10)]
    public string DestPort { get; set; }

    [Column("RateBasis")]
    [MaxLength(10)]
    public string RateBasis { get; set; }

    [Column("RatePerWm")]
    [Precision(18, 6)]
    public decimal? RatePerWm { get; set; }

    [Column("RatePerLb")]
    [Precision(18, 6)]
    public decimal? RatePerLb { get; set; }

    [Column("MinimumCharge")]
    [Precision(18, 6)]
    public decimal MinimumCharge { get; set; }

    [Column("BreakpointsJson")]
    public string? BreakpointsJson { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
