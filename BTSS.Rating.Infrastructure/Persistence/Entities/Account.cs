using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("Accounts", Schema="rating")]
public sealed class Account
{
    [Key]
    [Column("AccountId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AccountId { get; set; }

    [Column("AccountCode")]
    [MaxLength(50)]
    public string AccountCode { get; set; }

    [Column("Name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("Status")]
    [MaxLength(20)]
    public string Status { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
