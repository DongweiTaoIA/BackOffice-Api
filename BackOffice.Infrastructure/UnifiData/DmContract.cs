using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Infrastructure.UnifiData;

[Table("dmContract")]
public class DmContract
{
    [Key]
    [Column("ContractKey")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ContractKey { get; set; }

    [Column("ContractNum", TypeName = "char(12)")]
    [StringLength(12)]
    public string ContractNum { get; set; } = string.Empty;

    [Column("CompanyId")]
    [StringLength(4)]
    public string CompanyId { get; set; } = string.Empty;

    [Column("ProductId", TypeName = "char(2)")]
    [StringLength(2)]
    public string ProductId { get; set; } = string.Empty;

    [Column("DealerId")]
    [StringLength(8)]
    public string DealerId { get; set; } = string.Empty;

    [Column("ContractType")]
    [StringLength(2)]
    public string ContractType { get; set; } = string.Empty;

    [Column("ProgramId")]
    [StringLength(5)]
    public string? ProgramId { get; set; }

    [Column("ContractStatPri", TypeName = "char(1)")]
    [StringLength(1)]
    public string ContractStatPri { get; set; } = string.Empty;

    [Column("ContractStatSec", TypeName = "char(1)")]
    [StringLength(1)]
    public string ContractStatSec { get; set; } = string.Empty;

    [Column("CreateDt")]
    public DateTime CreateDt { get; set; }

    [Column("CreatedBy")]
    [StringLength(12)]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("CalcDt")]
    public DateTime? CalcDt { get; set; }

    [Column("FinalDt")]
    public DateTime? FinalDt { get; set; }

    [Column("FinalBy")]
    [StringLength(12)]
    public string? FinalBy { get; set; }

    [Column("EffectDt")]
    public DateTime EffectDt { get; set; }

    [Column("EffectKm")]
    public int? EffectKm { get; set; }

    [Column("ExpiryDt")]
    public DateTime? ExpiryDt { get; set; }

    [Column("ExpiryKm")]
    public int? ExpiryKm { get; set; }

    [Column("LastTransferDt")]
    public DateTime? LastTransferDt { get; set; }

    [Column("LastCancelDt")]
    public DateTime? LastCancelDt { get; set; }

    [Column("LastReinstateDt")]
    public DateTime? LastReinstateDt { get; set; }

    [Column("VehicleKey")]
    public int? VehicleKey { get; set; }

    [Column("NumOfKm")]
    public int? NumOfKm { get; set; }

    [Column("NumOfMiles")]
    public int? NumOfMiles { get; set; }

    [Column("PurchaseDt")]
    public DateTime? PurchaseDt { get; set; }

    [Column("DeliveryDt")]
    public DateTime? DeliveryDt { get; set; }

    [Column("VehiclePrice", TypeName = "money")]
    public decimal VehiclePrice { get; set; }

    [Column("VehicleRebate", TypeName = "money")]
    public decimal? VehicleRebate { get; set; }

    [Column("LicensePlate")]
    [StringLength(10)]
    public string? LicensePlate { get; set; }

    [Column("StockNum")]
    [StringLength(20)]
    public string? StockNum { get; set; }

    [Column("VehicleCondition")]
    [StringLength(2)]
    public string VehicleCondition { get; set; } = string.Empty;

    [Column("ClassCode")]
    [StringLength(2)]
    public string? ClassCode { get; set; }

    [Column("ImportYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? ImportYN { get; set; }

    [Column("ClaimOption", TypeName = "char(1)")]
    [StringLength(1)]
    public string? ClaimOption { get; set; }

    [Column("CommercialYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? CommercialYN { get; set; }

    [Column("CompanyName")]
    [StringLength(100)]
    public string? CompanyName { get; set; }

    [Column("CompanyRepKey")]
    public int? CompanyRepKey { get; set; }

    [Column("FinancingType", TypeName = "char(1)")]
    [StringLength(1)]
    public string? FinancingType { get; set; }

    [Column("FinancedAmt", TypeName = "money")]
    public decimal? FinancedAmt { get; set; }

    [Column("DownPaymentAmt", TypeName = "money")]
    public decimal? DownPaymentAmt { get; set; }

    [Column("PromoValue", TypeName = "char(4)")]
    [StringLength(4)]
    public string? PromoValue { get; set; }

    [Column("APR", TypeName = "numeric(5, 4)")]
    public decimal? APR { get; set; }

    [Column("LienHolderId")]
    [StringLength(2)]
    public string? LienHolderId { get; set; }

    [Column("LienHolderLabel")]
    [StringLength(255)]
    public string? LienHolderLabel { get; set; }

    [Column("LienHolderBranchId")]
    [StringLength(9)]
    public string? LienHolderBranchId { get; set; }

    [Column("FinancialInstId")]
    [StringLength(2)]
    public string? FinancialInstId { get; set; }

    [Column("FinancialInstLabel")]
    [StringLength(255)]
    public string? FinancialInstLabel { get; set; }

    [Column("PremiumFinInst")]
    [StringLength(7)]
    public string? PremiumFinInst { get; set; }

    [Column("Customer1Key")]
    public int? Customer1Key { get; set; }

    [Column("Customer2Key")]
    public int? Customer2Key { get; set; }

    [Column("IsAboriginalYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string IsAboriginalYN { get; set; } = "N";

    [Column("AboriginalCardNum")]
    [StringLength(10)]
    public string? AboriginalCardNum { get; set; }

    [Column("IsBuyerResidesOnReserveYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? IsBuyerResidesOnReserveYN { get; set; }

    [Column("IsBuyerDeliverToReserveYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string? IsBuyerDeliverToReserveYN { get; set; }

    [Column("Language", TypeName = "char(1)")]
    [StringLength(1)]
    public string Language { get; set; } = string.Empty;

    [Column("PaymentFreq", TypeName = "char(1)")]
    [StringLength(1)]
    public string? PaymentFreq { get; set; }

    [Column("PaymentStat")]
    [StringLength(2)]
    public string PaymentStat { get; set; } = string.Empty;

    [Column("PaymentMeth", TypeName = "char(1)")]
    [StringLength(1)]
    public string? PaymentMeth { get; set; }

    [Column("IsReceivedYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string IsReceivedYN { get; set; } = "N";

    [Column("FormRevKey")]
    public int? FormRevKey { get; set; }

    [Column("ConsentFormRevKey")]
    public int? ConsentFormRevKey { get; set; }

    [Column("ContractSource")]
    [StringLength(2)]
    public string ContractSource { get; set; } = string.Empty;

    [Column("ExtContractNum")]
    [StringLength(25)]
    public string? ExtContractNum { get; set; }

    [Column("IsVehicleRegisteredYN", TypeName = "char(1)")]
    [StringLength(1)]
    public string IsVehicleRegisteredYN { get; set; } = "N";

    [Column("MespMonths")]
    public short? MespMonths { get; set; }

    [Column("MespKm")]
    public int? MespKm { get; set; }

    [Column("BrokerId")]
    [StringLength(20)]
    public string? BrokerId { get; set; }

    [Column("BrokerName")]
    [StringLength(255)]
    public string? BrokerName { get; set; }

    [Column("ModDtTime")]
    public DateTime ModDtTime { get; set; }

    [Column("ModLoginId")]
    [StringLength(12)]
    public string ModLoginId { get; set; } = string.Empty;

    [Column("ComputedFinanceType", TypeName = "char(1)")]
    [StringLength(1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? ComputedFinanceType { get; set; }
}
