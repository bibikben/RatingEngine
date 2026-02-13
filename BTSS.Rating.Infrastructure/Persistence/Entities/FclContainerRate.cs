using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("FclContainerRates", Schema="rating")]
public sealed class FclContainerRate
{
    [Key]
    [Column("FclContainerRateId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long FclContainerRateId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("OriginPort")]
    [MaxLength(10)]
    public string OriginPort { get; set; }

    [Column("DestPort")]
    [MaxLength(10)]
    public string DestPort { get; set; }

    [Column("ContainerType")]
    [MaxLength(20)]
    public string ContainerType { get; set; }

    [Column("BaseRate")]
    [Precision(18, 6)]
    public decimal BaseRate { get; set; }

    [Column("FreeDays")]
    public int? FreeDays { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
