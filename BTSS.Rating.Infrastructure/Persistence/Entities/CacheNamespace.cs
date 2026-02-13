using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("CacheNamespaces", Schema="rating")]
public sealed class CacheNamespace
{
    [Key]
    [Column("NamespaceKey")]
    [MaxLength(200)]
    public string NamespaceKey { get; set; }

    [Column("VersionNo")]
    public long VersionNo { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Column("UpdatedBy")]
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    [Column("Note")]
    [MaxLength(500)]
    public string? Note { get; set; }
}
