using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("dmClaim")]
public class DmClaim
{
    [Key]
    [Column("ClaimKey")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClaimKey { get; set; }

    [Column("ClaimNum", TypeName = "char(10)")]
    [StringLength(10)]
    public string ClaimNum { get; set; } = string.Empty;

    [Column("ContractKey")]
    public int ContractKey { get; set; }

    [Column("ClaimType", TypeName = "char(1)")]
    [StringLength(1)]
    public string? ClaimType { get; set; }

    [Column("LossDt")]
    public DateTime LossDt { get; set; }

    [Column("LossType", TypeName = "char(2)")]
    [StringLength(2)]
    public string? LossType { get; set; }

    [Column("ClaimDt")]
    public DateTime ClaimDt { get; set; }

    [Column("EClaimReadyDt")]
    public DateTime? EClaimReadyDt { get; set; }

    [Column("SubmitDt")]
    public DateTime? SubmitDt { get; set; }

    [Column("OpenDt")]
    public DateTime? OpenDt { get; set; }

    [Column("ClaimStatPri", TypeName = "char(1)")]
    [StringLength(1)]
    public string ClaimStatPri { get; set; } = string.Empty;

    [Column("ClaimStatSec", TypeName = "char(1)")]
    [StringLength(1)]
    public string ClaimStatSec { get; set; } = string.Empty;

    [Column("AdjusterId")]
    [StringLength(12)]
    public string? AdjusterId { get; set; }

    [Column("AdjudPriority", TypeName = "char(1)")]
    [StringLength(1)]
    public string? AdjudPriority { get; set; }

    [Column("AdjudStat")]
    [StringLength(2)]
    public string? AdjudStat { get; set; }

    [Column("ClosedDt")]
    public DateTime? ClosedDt { get; set; }

    [Column("NumOfKm")]
    public int? NumOfKm { get; set; }

    [Column("NumOfMiles")]
    public int? NumOfMiles { get; set; }

    [Column("LiabilityLimit", TypeName = "money")]
    public decimal? LiabilityLimit { get; set; }

    [Column("OverrideYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? OverrideYN { get; set; }

    [Column("OverrideBy")]
    [StringLength(12)]
    public string? OverrideBy { get; set; }

    [Column("OverrideDt")]
    public DateTime? OverrideDt { get; set; }

    [Column("LicensePlate")]
    [StringLength(10)]
    public string? LicensePlate { get; set; }

    [Column("RONum")]
    [StringLength(20)]
    public string? RONum { get; set; }

    [Column("ContactName")]
    [StringLength(50)]
    public string? ContactName { get; set; }

    [Column("ContactPhoneNum")]
    [StringLength(20)]
    public string? ContactPhoneNum { get; set; }

    [Column("ContactPhoneExt")]
    [StringLength(4)]
    public string? ContactPhoneExt { get; set; }

    [Column("ContactFaxNum")]
    [StringLength(20)]
    public string? ContactFaxNum { get; set; }

    [Column("ContactEmail")]
    [StringLength(100)]
    public string? ContactEmail { get; set; }

    [Column("RepairCenter")]
    [StringLength(100)]
    public string? RepairCenter { get; set; }

    [Column("Comments")]
    [StringLength(1000)]
    public string? Comments { get; set; }

    [Column("VerifyHistoryYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? VerifyHistoryYN { get; set; }

    [Column("VehicleHistoryVerifiedYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? VehicleHistoryVerifiedYN { get; set; }

    [Column("ExtClaimNum")]
    [StringLength(25)]
    public string? ExtClaimNum { get; set; }

    [Column("ClaimDealerId")]
    [StringLength(8)]
    public string? ClaimDealerId { get; set; }

    [Column("PreferredLanguage", TypeName = "char(1)")]
    [StringLength(1)]
    public string PreferredLanguage { get; set; } = "E";

    [Column("DataSource")]
    [StringLength(2)]
    public string? DataSource { get; set; }

    [Column("CreatedBy")]
    [StringLength(12)]
    public string? CreatedBy { get; set; }

    [Column("CreatedDt")]
    public DateTime? CreatedDt { get; set; }

    [Column("ModDtTime")]
    public DateTime ModDtTime { get; set; }

    [Column("ModLoginId")]
    [StringLength(12)]
    public string ModLoginId { get; set; } = string.Empty;
}
