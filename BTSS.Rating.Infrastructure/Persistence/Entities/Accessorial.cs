using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("Accessorials", Schema="rating")]
public sealed class Accessorial
{
    [Key]
    [Column("AccessorialId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AccessorialId { get; set; }

    [Column("Code")]
    [MaxLength(50)]
    public string Code { get; set; }

    [Column("Description")]
    [MaxLength(200)]
    public string? Description { get; set; }

    [Column("ModeApplicabilityJson")]
    public string? ModeApplicabilityJson { get; set; }
}
