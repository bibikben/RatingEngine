using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("ContractAccessorialCharges", Schema="rating")]
public sealed class ContractAccessorialCharge
{
    [Key]
    [Column("ContractAccessorialChargeId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractAccessorialChargeId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("AccessorialId")]
    public long AccessorialId { get; set; }

    [Column("CalcType")]
    [MaxLength(20)]
    public string CalcType { get; set; }

    [Column("FlatAmount")]
    [Precision(18, 6)]
    public decimal? FlatAmount { get; set; }

    [Column("PercentValue")]
    [Precision(18, 6)]
    public decimal? PercentValue { get; set; }

    [Column("FormulaExpression")]
    public string? FormulaExpression { get; set; }

    [Column("TierTableJson")]
    public string? TierTableJson { get; set; }

    [Column("ApplyTo")]
    [MaxLength(30)]
    public string ApplyTo { get; set; }

    [Column("MinAmount")]
    [Precision(18, 6)]
    public decimal? MinAmount { get; set; }

    [Column("MaxAmount")]
    [Precision(18, 6)]
    public decimal? MaxAmount { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
