using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTSS.Rating.Infrastructure.Persistence.Entities;

[Table("AccessorialRules", Schema="rating")]
public sealed class AccessorialRule
{
    [Key]
    [Column("AccessorialRuleId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AccessorialRuleId { get; set; }

    [Column("ContractVersionId")]
    public long ContractVersionId { get; set; }

    [Column("AccessorialId")]
    public long AccessorialId { get; set; }

    [Column("RuleName")]
    [MaxLength(200)]
    public string RuleName { get; set; }

    [Column("ConditionExpression")]
    public string ConditionExpression { get; set; }

    [Column("OverrideCalcType")]
    [MaxLength(20)]
    public string? OverrideCalcType { get; set; }

    [Column("OverrideExpression")]
    public string? OverrideExpression { get; set; }

    [Column("StackingPolicy")]
    [MaxLength(20)]
    public string StackingPolicy { get; set; }

    [Column("ExclusiveGroupKey")]
    [MaxLength(50)]
    public string? ExclusiveGroupKey { get; set; }

    [Column("Priority")]
    public int Priority { get; set; }

    [Column("IsEnabled")]
    public bool IsEnabled { get; set; }

    [Column("Note")]
    public string? Note { get; set; }

    [Column("EffectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("ExpirationDate")]
    public DateOnly ExpirationDate { get; set; }
}
