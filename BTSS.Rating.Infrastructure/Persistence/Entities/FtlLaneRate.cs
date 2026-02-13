using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("FtlLaneRates", Schema="rating")]
public sealed class FtlLaneRate
{
    [Key]
    [Column("FtlLaneRateId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long FtlLaneRateId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("OriginRegionId")]
    public int OriginRegionId { get; set; }

    [Column("DestRegionId")]
    public int DestRegionId { get; set; }

    [Column("EquipmentType")]
    [MaxLength(30)]
    public string EquipmentType { get; set; }

    [Column("RateType")]
    [MaxLength(20)]
    public string RateType { get; set; }

    [Column("RateValue")]
    [Precision(18, 6)]
    public decimal RateValue { get; set; }

    [Column("MinimumCharge")]
    [Precision(18, 6)]
    public decimal? MinimumCharge { get; set; }

    [Column("IncludedStops")]
    public int IncludedStops { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
