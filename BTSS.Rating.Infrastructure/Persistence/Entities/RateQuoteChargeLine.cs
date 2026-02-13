using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("RateQuoteChargeLines", Schema="rating")]
public sealed class RateQuoteChargeLine
{
    [Key]
    [Column("RateQuoteChargeLineId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RateQuoteChargeLineId { get; set; }

    [Column("RateQuoteResultId")]
    public long RateQuoteResultId { get; set; }

    [Column("SequenceNo")]
    public int SequenceNo { get; set; }

    [Column("CanonicalChargeType")]
    [MaxLength(30)]
    public string CanonicalChargeType { get; set; }

    [Column("AccessorialCode")]
    [MaxLength(50)]
    public string? AccessorialCode { get; set; }

    [Column("EdiStandard")]
    [MaxLength(10)]
    public string? EdiStandard { get; set; }

    [Column("EdiChargeCode")]
    [MaxLength(50)]
    public string? EdiChargeCode { get; set; }

    [Column("Description")]
    [MaxLength(200)]
    public string? Description { get; set; }

    [Column("Quantity")]
    [Precision(18, 6)]
    public decimal? Quantity { get; set; }

    [Column("Rate")]
    [Precision(18, 6)]
    public decimal? Rate { get; set; }

    [Column("Amount")]
    [Precision(18, 6)]
    public decimal Amount { get; set; }

    [Column("ApplyTo")]
    [MaxLength(30)]
    public string? ApplyTo { get; set; }

    [Column("StopSequence")]
    public int? StopSequence { get; set; }

    [Column("LegSequence")]
    public int? LegSequence { get; set; }

    [Column("DetailJson")]
    public string? DetailJson { get; set; }
}
