using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("dmDealer")]
public class DmDealer
{
    [Key]
    [Column("DealerId")]
    [StringLength(8)]
    public string DealerId { get; set; } = string.Empty;

    [Column("DealerStat")]
    [StringLength(1)]
    public string DealerStat { get; set; } = string.Empty;

    [Column("DBAName")]
    [StringLength(100)]
    public string DBAName { get; set; } = string.Empty;

    [Column("DealerGroup")]
    [StringLength(7)]
    public string? DealerGroup { get; set; }

    [Column("TerritoryId")]
    [StringLength(5)]
    public string TerritoryId { get; set; } = string.Empty;

    [Column("Language")]
    [StringLength(1)]
    public string? Language { get; set; }

    [Column("DealerCatg")]
    [StringLength(2)]
    public string? DealerCatg { get; set; }

    [Column("IsDealershipYN")]
    [StringLength(1)]
    public string IsDealershipYN { get; set; } = string.Empty;

    [Column("IsBrokerYN")]
    [StringLength(1)]
    public string IsBrokerYN { get; set; } = string.Empty;

    [Column("City")]
    [StringLength(50)]
    public string? City { get; set; }

    [Column("ProvState")]
    [StringLength(2)]
    public string? ProvState { get; set; }

    [Column("PostalZip")]
    [StringLength(10)]
    public string? PostalZip { get; set; }

    [Column("PhoneNum")]
    [StringLength(20)]
    public string? PhoneNum { get; set; }

    [Column("FaxNum")]
    [StringLength(20)]
    public string? FaxNum { get; set; }

    [Column("WebPageUrl")]
    [StringLength(255)]
    public string? WebPageUrl { get; set; }

    [Column("LegalName")]
    [StringLength(100)]
    public string? LegalName { get; set; }

    [Column("OEM")]
    [StringLength(2)]
    public string? OEM { get; set; }

    [Column("ProducerMake")]
    [StringLength(2)]
    public string? ProducerMake { get; set; }

    [Column("ProducerClass")]
    [StringLength(2)]
    public string? ProducerClass { get; set; }

    [Column("IsDemoYN")]
    [StringLength(1)]
    public string IsDemoYN { get; set; } = "N";
}
