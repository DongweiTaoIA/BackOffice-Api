using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("EW_cfDealerProgramMarkup")]
public class EwDealerProgramMarkup
{
    [Key]
    [Column("DealerMarkupKey")]
    public int DealerMarkupKey { get; set; }

    [Column("DealerId")]
    [StringLength(8)]
    public string DealerId { get; set; } = string.Empty;

    [Column("ProgramId")]
    [StringLength(5)]
    public string ProgramId { get; set; } = string.Empty;

    [Column("MarkupVal")]
    public decimal MarkupVal { get; set; }
}
