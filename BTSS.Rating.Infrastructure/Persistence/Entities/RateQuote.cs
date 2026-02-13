using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("RateQuotes", Schema="rating")]
public sealed class RateQuote
{
    [Key]
    [Column("RateQuoteId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RateQuoteId { get; set; }

    [Column("RequestId")]
    public Guid RequestId { get; set; }

    [Column("AccountId")]
    public long AccountId { get; set; }

    [Column("Mode")]
    [MaxLength(10)]
    public string Mode { get; set; }

    [Column("CurrencyCode")]
    [MaxLength(3)]
    public string CurrencyCode { get; set; }

    [Column("RateDate")]
    public DateOnly RateDate { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
