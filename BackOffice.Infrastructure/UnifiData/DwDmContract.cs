using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("DW_dmContract")]
public class DwDmContract
{
    [Key]
    [Column("ContractKey")]
    public int ContractKey { get; set; }

    [Column("DwProgramId")]
    [StringLength(5)]
    public string? DwProgramId { get; set; }
}
