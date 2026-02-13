using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("FuelSchedules", Schema="rating")]
public sealed class FuelSchedule
{
    [Key]
    [Column("FuelScheduleId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long FuelScheduleId { get; set; }

    [Column("Name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("IndexType")]
    [MaxLength(20)]
    public string IndexType { get; set; }

    [Column("Unit")]
    [MaxLength(20)]
    public string Unit { get; set; }

    [Column("Notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
}
