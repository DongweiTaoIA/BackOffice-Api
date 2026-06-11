using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("cfProgram")]
public class CfProgram
{
    [Key]
    [Column("ProgramId")]
    [StringLength(5)]
    public string ProgramId { get; set; } = string.Empty;

    [Column("ProgramType")]
    [StringLength(1)]
    public string? ProgramType { get; set; }

    [Column("DescnEn")]
    [StringLength(50)]
    public string DescnEn { get; set; } = string.Empty;

    [Column("DescnFr")]
    [StringLength(50)]
    public string? DescnFr { get; set; }

    [Column("ContractGroup")]
    [StringLength(3)]
    public string ContractGroup { get; set; } = string.Empty;

    [Column("ClassGroup")]
    [StringLength(5)]
    public string ClassGroup { get; set; } = string.Empty;

    [Column("Make")]
    [StringLength(25)]
    public string? Make { get; set; }

    [Column("SortOrder")]
    public short SortOrder { get; set; }

    [Column("IsAddOnYN")]
    [StringLength(1)]
    public string IsAddOnYN { get; set; } = "N";
}
