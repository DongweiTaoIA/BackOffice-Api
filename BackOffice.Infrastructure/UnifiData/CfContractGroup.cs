using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("cfContractGroup")]
public class CfContractGroup
{
    [Key]
    [Column("ContractGroup")]
    [StringLength(3)]
    public string ContractGroup { get; set; } = string.Empty;

    [Column("ProductId")]
    [StringLength(2)]
    public string ProductId { get; set; } = string.Empty;
}
