namespace BackOffice.Domain.Models;

public class ChatDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MessageDto> Messages { get; set; } = [];
}

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EligibilityResult? EligibilityResult { get; set; }
    public List<SuggestedAction>? Suggestions { get; set; }
}

public class SuggestedAction
{
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

public class EligibilityResult
{
    public bool IsEligible { get; set; }
    public string DealerCode { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    /// <summary>Raw dealer status code (e.g. "A" for active, "I" for inactive). Null if dealer was not found.</summary>
    public string? DealerStatus { get; set; }
    /// <summary>Display label for the product as it appeared in the query (e.g. "Extended Warranty" or "EW").</summary>
    public string Product { get; set; } = string.Empty;
    /// <summary>Canonical product code resolved by the registry (e.g. "EW", "DW"). Always a stable code, never a display name.</summary>
    public string? ProductId { get; set; }
    /// <summary>Program code resolved from the query (e.g. "EWXX001"). Populated when the query referenced a specific program (by ProgramId or ProgramName).</summary>
    public string? ProgramId { get; set; }
    /// <summary>Program display name resolved from the query (e.g. "Retail Wearable Parts").</summary>
    public string? ProgramName { get; set; }
    public string Environment { get; set; } = string.Empty;
    public List<ProgramInfo> Programs { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}

public class ProgramInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameFr { get; set; }
    public string Status { get; set; } = "Active";
}

public class EligibilityQuery
{
    public string? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProgramId { get; set; }
    public string? ProgramName { get; set; }
}

public class ActivateRequest
{
    public string DealerId { get; set; } = string.Empty;
    /// <summary>Optional product code (e.g. "EW"). When provided, the service uses it to disambiguate the program.</summary>
    public string? ProductId { get; set; }
    public string ProgramId { get; set; } = string.Empty;
    public string? EffectiveDate { get; set; }
}

public class DeactivateRequest
{
    public string DealerId { get; set; } = string.Empty;
    /// <summary>Optional product code (e.g. "EW"). When provided, the service uses it to disambiguate the program.</summary>
    public string? ProductId { get; set; }
    public string ProgramId { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
}

public class ProgramActionResult
{
    public bool Success { get; set; }
    public string DealerCode { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class CancellationEligibilityResult
{
    public bool IsEligible { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public string RefundType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    /// <summary>Messages returned by the cancellation-calc stored procedure (typically a warning or rule explanation).</summary>
    public List<CancellationMessage>? Messages { get; set; }
    /// <summary>Detailed refund breakdown returned by the cancellation-calc stored procedure.</summary>
    public CancellationRefundDetails? RefundDetails { get; set; }
}

public class CancellationMessage
{
    public string MsgText { get; set; } = string.Empty;
    /// <summary>Single-char message type from the SP (e.g. "E" for error/explanation, "W" for warning, "I" for info).</summary>
    public string MsgType { get; set; } = string.Empty;
}

public class CancellationRefundDetails
{
    public decimal? RetailPremiumPaidAmount { get; set; }
    /// <summary>Gross refund (before admin fee, claims, and taxes).</summary>
    public decimal? RefundAmount { get; set; }
    /// <summary>Refund factor as returned by the SP (e.g. 0.6881 or 68.81 depending on the SP's scale).</summary>
    public decimal? Factor { get; set; }
    public decimal? ClaimsPaidAmount { get; set; }
    public decimal? AdminFee { get; set; }
    /// <summary>Net refund after admin fee and claims, but before sales taxes.</summary>
    public decimal? NetRefundAmount { get; set; }
    /// <summary>GST/HST portion.</summary>
    public decimal? RefundTax1Amount { get; set; }
    /// <summary>PST/IPT portion.</summary>
    public decimal? RefundTax2Amount { get; set; }
    /// <summary>Total refund paid to the customer. Computed as NetRefundAmount + RefundTax1Amount + RefundTax2Amount; not returned by the SP.</summary>
    public decimal? TotalRefund { get; set; }

    // ---- Dealer chargeback side, returned by DPP_DP612ContractCancel_Calc ----
    public decimal? DealerMarkupAmount { get; set; }
    public decimal? DealerMarkupPercentage { get; set; }
    public decimal? NetDealerChargebackAmount { get; set; }
    public decimal? ChargebackTax1Amount { get; set; }
    public decimal? ChargebackTax2Amount { get; set; }
    public decimal? DealerChargebackAmount { get; set; }
    public decimal? IapPortionAmount { get; set; }

    // ---- Tax & cheque flags ----
    public decimal? Tax1Value { get; set; }
    public decimal? Tax2Value { get; set; }
    public string? EnableIssueChequeYN { get; set; }
    public string? Tax1RemitYN { get; set; }
    public string? Tax2RemitYN { get; set; }
}

public class MaxMarkupResult
{
    public bool Found { get; set; }
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public decimal MaxMarkup { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class ProgramLookupResult
{
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string? ProgramNameFr { get; set; }
    /// <summary>The contract group this program belongs to (cfProgram.ContractGroup).</summary>
    public string ContractGroup { get; set; } = string.Empty;
    /// <summary>The product this program rolls up to (cfProduct.ProductId via cfContractGroup), e.g. "EW", "DW".</summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>The product display name (cfProduct.DescnEn).</summary>
    public string ProductName { get; set; } = string.Empty;
}

public class DealerSearchResult
{
    public string DealerId { get; set; } = string.Empty;
    public string DBAName { get; set; } = string.Empty;
    public string DealerStat { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? ProvState { get; set; }
}

public class DealerPagedResult
{
    public List<DealerSearchResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class DealerDetailsDto
{
    public string DealerId { get; set; } = string.Empty;
    public string DBAName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string DealerStat { get; set; } = string.Empty;
    public string? DealerGroup { get; set; }
    public string? DealerCatg { get; set; }
    public string? City { get; set; }
    public string? ProvState { get; set; }
    public string? PostalZip { get; set; }
    public string? PhoneNum { get; set; }
    public string? FaxNum { get; set; }
    public string? WebPageUrl { get; set; }
    public string TerritoryId { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? OEM { get; set; }
    public string IsDealershipYN { get; set; } = string.Empty;
    public string IsBrokerYN { get; set; } = string.Empty;
    public string? ProducerMake { get; set; }
    public string? ProducerClass { get; set; }
}

public class ContractSearchResult
{
    public int ContractKey { get; set; }
    public string ContractNum { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public string? DealerName { get; set; }
    public string ContractStatPri { get; set; } = string.Empty;
    public string ContractStatSec { get; set; } = string.Empty;
    public DateTime EffectDt { get; set; }
    public DateTime? ExpiryDt { get; set; }
    public string? ExtContractNum { get; set; }
}

public class ContractPagedResult
{
    public List<ContractSearchResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class ContractDetailsDto
{
    public int ContractKey { get; set; }
    public string ContractNum { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public string? DealerName { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public string? ProgramId { get; set; }
    public string ContractStatPri { get; set; } = string.Empty;
    public string ContractStatSec { get; set; } = string.Empty;
    public DateTime CreateDt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CalcDt { get; set; }
    public DateTime? FinalDt { get; set; }
    public string? FinalBy { get; set; }
    public DateTime EffectDt { get; set; }
    public int? EffectKm { get; set; }
    public DateTime? ExpiryDt { get; set; }
    public int? ExpiryKm { get; set; }
    public DateTime? LastTransferDt { get; set; }
    public DateTime? LastCancelDt { get; set; }
    public DateTime? LastReinstateDt { get; set; }
    public int? VehicleKey { get; set; }
    public int? NumOfKm { get; set; }
    public int? NumOfMiles { get; set; }
    public DateTime? PurchaseDt { get; set; }
    public DateTime? DeliveryDt { get; set; }
    public decimal VehiclePrice { get; set; }
    public decimal? VehicleRebate { get; set; }
    public string? LicensePlate { get; set; }
    public string? StockNum { get; set; }
    public string VehicleCondition { get; set; } = string.Empty;
    public string? ClassCode { get; set; }
    public string? ImportYN { get; set; }
    public string? ClaimOption { get; set; }
    public string? CommercialYN { get; set; }
    public string? CompanyName { get; set; }
    public int? CompanyRepKey { get; set; }
    public string? FinancingType { get; set; }
    public decimal? FinancedAmt { get; set; }
    public decimal? DownPaymentAmt { get; set; }
    public string? PromoValue { get; set; }
    public decimal? APR { get; set; }
    public string? LienHolderId { get; set; }
    public string? LienHolderLabel { get; set; }
    public string? LienHolderBranchId { get; set; }
    public string? FinancialInstId { get; set; }
    public string? FinancialInstLabel { get; set; }
    public string? PremiumFinInst { get; set; }
    public int? Customer1Key { get; set; }
    public int? Customer2Key { get; set; }
    public string IsAboriginalYN { get; set; } = "N";
    public string? AboriginalCardNum { get; set; }
    public string? IsBuyerResidesOnReserveYN { get; set; }
    public string? IsBuyerDeliverToReserveYN { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? PaymentFreq { get; set; }
    public string PaymentStat { get; set; } = string.Empty;
    public string? PaymentMeth { get; set; }
    public string IsReceivedYN { get; set; } = "N";
    public int? FormRevKey { get; set; }
    public int? ConsentFormRevKey { get; set; }
    public string ContractSource { get; set; } = string.Empty;
    public string? ExtContractNum { get; set; }
    public string IsVehicleRegisteredYN { get; set; } = "N";
    public short? MespMonths { get; set; }
    public int? MespKm { get; set; }
    public string? BrokerId { get; set; }
    public string? BrokerName { get; set; }
    public DateTime ModDtTime { get; set; }
    public string ModLoginId { get; set; } = string.Empty;
    public string? ComputedFinanceType { get; set; }

    // ----- Enriched (populated via follow-up lookups on dmCustomer / dmVehicle / dmClaim) -----
    public string? Customer1Name { get; set; }
    public string? Customer2Name { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerProvState { get; set; }
    public string? CustomerPostalZip { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public string? Vin { get; set; }
    public short? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleOdometer { get; set; }

    public int OpenClaimCount { get; set; }
    public int TotalClaimCount { get; set; }
}

public class ClaimSearchResult
{
    public int ClaimKey { get; set; }
    public string ClaimNum { get; set; } = string.Empty;
    public int ContractKey { get; set; }
    public string? ContractNum { get; set; }
    public string? ClaimType { get; set; }
    public DateTime LossDt { get; set; }
    public DateTime ClaimDt { get; set; }
    public string ClaimStatPri { get; set; } = string.Empty;
    public string ClaimStatSec { get; set; } = string.Empty;
    public string? AdjusterId { get; set; }
    public string? ClaimDealerId { get; set; }
    public string? DealerName { get; set; }
    public string? RONum { get; set; }
    public string? ExtClaimNum { get; set; }
}

public class ClaimPagedResult
{
    public List<ClaimSearchResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class ClaimDetailsDto
{
    public int ClaimKey { get; set; }
    public string ClaimNum { get; set; } = string.Empty;
    public int ContractKey { get; set; }
    public string? ContractNum { get; set; }
    public string? ClaimType { get; set; }
    public DateTime LossDt { get; set; }
    public string? LossType { get; set; }
    public DateTime ClaimDt { get; set; }
    public DateTime? EClaimReadyDt { get; set; }
    public DateTime? SubmitDt { get; set; }
    public DateTime? OpenDt { get; set; }
    public string ClaimStatPri { get; set; } = string.Empty;
    public string ClaimStatSec { get; set; } = string.Empty;
    public string? AdjusterId { get; set; }
    public string? AdjudPriority { get; set; }
    public string? AdjudStat { get; set; }
    public DateTime? ClosedDt { get; set; }
    public int? NumOfKm { get; set; }
    public int? NumOfMiles { get; set; }
    public decimal? LiabilityLimit { get; set; }
    public string? OverrideYN { get; set; }
    public string? OverrideBy { get; set; }
    public DateTime? OverrideDt { get; set; }
    public string? LicensePlate { get; set; }
    public string? RONum { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhoneNum { get; set; }
    public string? ContactPhoneExt { get; set; }
    public string? ContactFaxNum { get; set; }
    public string? ContactEmail { get; set; }
    public string? RepairCenter { get; set; }
    public string? Comments { get; set; }
    public string? VerifyHistoryYN { get; set; }
    public string? VehicleHistoryVerifiedYN { get; set; }
    public string? ExtClaimNum { get; set; }
    public string? ClaimDealerId { get; set; }
    public string? DealerName { get; set; }
    public string PreferredLanguage { get; set; } = "E";
    public string? DataSource { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDt { get; set; }
    public DateTime ModDtTime { get; set; }
    public string ModLoginId { get; set; } = string.Empty;
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ChatId { get; set; }
}

public class CreateChatRequest
{
    public string Title { get; set; } = "New Chat";
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ReportBugRequest
{
    public string Description { get; set; } = string.Empty;
    public string? ChatId { get; set; }
    public string? MessageId { get; set; }
}

public class ContractStatusDto
{
    public string ContractId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = "dmContract";
}
