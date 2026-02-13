using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("EdiChargeCodes", Schema="rating")]
public sealed class EdiChargeCode
{
    [Key]
    [Column("EdiChargeCodeId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long EdiChargeCodeId { get; set; }

    [Column("Standard")]
    [MaxLength(10)]
    public string Standard { get; set; }

    [Column("Code")]
    [MaxLength(50)]
    public string Code { get; set; }

    [Column("Description")]
    [MaxLength(200)]
    public string? Description { get; set; }

    [Column("CanonicalChargeType")]
    [MaxLength(30)]
    public string CanonicalChargeType { get; set; }

    [Column("DefaultAccessorialCode")]
    [MaxLength(50)]
    public string? DefaultAccessorialCode { get; set; }
}
