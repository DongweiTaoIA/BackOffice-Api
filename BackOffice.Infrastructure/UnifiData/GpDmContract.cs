using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("GP_dmContract")]
public class GpDmContract
{
    [Key]
    [Column("ContractKey")]
    public int ContractKey { get; set; }

    [Column("GpProgramId")]
    [StringLength(5)]
    public string? GpProgramId { get; set; }
}
