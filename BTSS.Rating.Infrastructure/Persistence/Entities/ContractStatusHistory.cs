using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("ContractStatusHistory", Schema="rating")]
public sealed class ContractStatusHistory
{
    [Key]
    [Column("ContractStatusHistoryId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractStatusHistoryId { get; set; }

    [Column("ContractId")]
    public long ContractId { get; set; }

    [Column("ChangedAt")]
    public DateTime ChangedAt { get; set; }

    [Column("FromStatus")]
    [MaxLength(20)]
    public string? FromStatus { get; set; }

    [Column("ToStatus")]
    [MaxLength(20)]
    public string ToStatus { get; set; }

    [Column("UserId")]
    [MaxLength(450)]
    public string UserId { get; set; }

    [Column("Note")]
    public string? Note { get; set; }
}
