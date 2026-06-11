using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("EW_cfDealerProgram")]
public class EwDealerProgram
{
    [Key]
    [Column("DealerProgramKey")]
    public int DealerProgramKey { get; set; }

    [Column("DealerId")]
    [StringLength(8)]
    public string DealerId { get; set; } = string.Empty;

    [Column("ProgramId")]
    [StringLength(5)]
    public string ProgramId { get; set; } = string.Empty;

    [Column("DlrPortalYN")]
    [StringLength(1)]
    public string DlrPortalYN { get; set; } = string.Empty;

    [Column("AdminDBYN")]
    [StringLength(1)]
    public string AdminDBYN { get; set; } = string.Empty;

    [Column("ExtDealerId")]
    [StringLength(20)]
    public string? ExtDealerId { get; set; }

    [Column("EffectDt")]
    public DateTime EffectDt { get; set; }

    [Column("ExpiryDt")]
    public DateTime ExpiryDt { get; set; }
}
