using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("GeoZipZone", Schema="rating")]
public sealed class GeoZipZone
{
    [Key]
    [Column("GeoZipZoneId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long GeoZipZoneId { get; set; }

    [Column("PostalCode")]
    [MaxLength(20)]
    public string PostalCode { get; set; }

    [Column("CountryCode")]
    [MaxLength(2)]
    public string CountryCode { get; set; }

    [Column("ZoneId")]
    public int? ZoneId { get; set; }

    [Column("RegionId")]
    public int? RegionId { get; set; }

    [Column("MetroId")]
    public int? MetroId { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }
}
