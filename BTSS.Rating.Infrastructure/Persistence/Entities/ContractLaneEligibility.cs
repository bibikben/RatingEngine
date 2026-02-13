using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("ContractLaneEligibility", Schema="rating")]
public sealed class ContractLaneEligibility
{
    [Key]
    [Column("ContractLaneEligibilityId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractLaneEligibilityId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("Mode")]
    [MaxLength(10)]
    public string Mode { get; set; }

    [Column("OriginZoneId")]
    public int? OriginZoneId { get; set; }

    [Column("DestZoneId")]
    public int? DestZoneId { get; set; }

    [Column("OriginRegionId")]
    public int? OriginRegionId { get; set; }

    [Column("DestRegionId")]
    public int? DestRegionId { get; set; }

    [Column("EquipmentType")]
    [MaxLength(30)]
    public string? EquipmentType { get; set; }

    [Column("OriginPort")]
    [MaxLength(10)]
    public string? OriginPort { get; set; }

    [Column("DestPort")]
    [MaxLength(10)]
    public string? DestPort { get; set; }

    [Column("ContainerType")]
    [MaxLength(20)]
    public string? ContainerType { get; set; }

    [Column("AllowsMultiStop")]
    public bool? AllowsMultiStop { get; set; }

    [Column("MaxStops")]
    public int? MaxStops { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("LaneKeyString")]
    [MaxLength(300)]
    public string LaneKeyString { get; set; }

    [Column("LaneKeyHash")]
    public byte[] LaneKeyHash { get; set; }
}
