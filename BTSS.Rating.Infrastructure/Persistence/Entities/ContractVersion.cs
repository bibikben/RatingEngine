using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("ContractVersions", Schema="rating")]
public sealed class ContractVersion
{
    [Key]
    [Column("ContractVersionId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractVersionId { get; set; }

    [Column("ContractId")]
    public long ContractId { get; set; }

    [Column("VersionNo")]
    public int VersionNo { get; set; }

    [Column("EffectiveStart")]
    public DateOnly EffectiveStart { get; set; }

    [Column("EffectiveEnd")]
    public DateOnly EffectiveEnd { get; set; }

    [Column("PublishStatus")]
    [MaxLength(20)]
    public string PublishStatus { get; set; }

    [Column("CreatedBy")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("PublishedAt")]
    public DateTime? PublishedAt { get; set; }

    [Column("Note")]
    public string? Note { get; set; }
}
