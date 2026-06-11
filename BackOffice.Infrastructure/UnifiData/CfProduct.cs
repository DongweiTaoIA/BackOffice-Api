using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("cfProduct")]
public class CfProduct
{
    [Key]
    [Column("ProductId")]
    [StringLength(2)]
    public string ProductId { get; set; } = string.Empty;

    [Column("DescnEn")]
    [StringLength(100)]
    public string DescnEn { get; set; } = string.Empty;

    [Column("DescnFr")]
    [StringLength(100)]
    public string? DescnFr { get; set; }

    [Column("PrfxQuoteNum")]
    [StringLength(2)]
    public string PrfxQuoteNum { get; set; } = string.Empty;

    [Column("PrfxClaimNum")]
    [StringLength(2)]
    public string PrfxClaimNum { get; set; } = string.Empty;

    [Column("IsRemittableYN")]
    [StringLength(1)]
    public string IsRemittableYN { get; set; } = "N";

    [Column("Source")]
    [StringLength(10)]
    public string? Source { get; set; } = "Internal";

    [Column("IsPayableYN")]
    [StringLength(1)]
    public string IsPayableYN { get; set; } = "N";
}
