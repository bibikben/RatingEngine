using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("RateQuoteResults", Schema="rating")]
public sealed class RateQuoteResult
{
    [Key]
    [Column("RateQuoteResultId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RateQuoteResultId { get; set; }

    [Column("RateQuoteId")]
    public long RateQuoteId { get; set; }

    [Column("ProviderId")]
    public long ProviderId { get; set; }

    [Column("ContractId")]
    public long ContractId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("Rank")]
    public int Rank { get; set; }

    [Column("TotalAmount")]
    [Precision(18, 6)]
    public decimal TotalAmount { get; set; }

    [Column("TransitDays")]
    public int? TransitDays { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
