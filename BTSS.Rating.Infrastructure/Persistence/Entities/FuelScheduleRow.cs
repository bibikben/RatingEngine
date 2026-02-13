using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("FuelScheduleRows", Schema="rating")]
public sealed class FuelScheduleRow
{
    [Key]
    [Column("FuelScheduleRowId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long FuelScheduleRowId { get; set; }

    [Column("FuelScheduleId")]
    public long FuelScheduleId { get; set; }

    [Column("EffectiveStart")]
    public DateOnly EffectiveStart { get; set; }

    [Column("EffectiveEnd")]
    public DateOnly EffectiveEnd { get; set; }

    [Column("IndexMin")]
    [Precision(18, 6)]
    public decimal? IndexMin { get; set; }

    [Column("IndexMax")]
    [Precision(18, 6)]
    public decimal? IndexMax { get; set; }

    [Column("FuelValue")]
    [Precision(18, 6)]
    public decimal FuelValue { get; set; }
}
