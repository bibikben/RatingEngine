using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("ContractFuelRules", Schema="rating")]
public sealed class ContractFuelRule
{
    [Key]
    [Column("ContractFuelRuleId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContractFuelRuleId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("FuelScheduleId")]
    public long FuelScheduleId { get; set; }

    [Column("ApplyTo")]
    [MaxLength(20)]
    public string ApplyTo { get; set; }

    [Column("CalcMethod")]
    [MaxLength(20)]
    public string CalcMethod { get; set; }

    [Column("FormulaExpression")]
    public string? FormulaExpression { get; set; }

    [Column("RoundingJson")]
    public string? RoundingJson { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
