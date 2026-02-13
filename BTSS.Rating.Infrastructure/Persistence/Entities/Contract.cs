using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("Contracts", Schema="rating")]
public sealed class Contract
{
    [Key]
    [Column("ContractId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractId { get; set; }

    [Column("AccountId")]
    public long? AccountId { get; set; }

    [Column("ProviderId")]
    public long ProviderId { get; set; }

    [Column("Mode")]
    [MaxLength(10)]
    public string Mode { get; set; }

    [Column("Name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("CurrencyCode")]
    [MaxLength(3)]
    public string CurrencyCode { get; set; }

    [Column("Status")]
    [MaxLength(20)]
    public string Status { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("Note")]
    public string? Note { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("PublishedDate")]
    public DateOnly? PublishedDate { get; set; }
}
