using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("Providers", Schema="rating")]
public sealed class Provider
{
    [Key]
    [Column("ProviderId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ProviderId { get; set; }

    [Column("ProviderCode")]
    [MaxLength(50)]
    public string ProviderCode { get; set; }

    [Column("Name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("Status")]
    [MaxLength(20)]
    public string Status { get; set; }

    [Column("ModeCapabilitiesJson")]
    public string? ModeCapabilitiesJson { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
