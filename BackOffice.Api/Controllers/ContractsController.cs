using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController(IContractService contractService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<List<ContractSearchResult>>> Search([FromQuery] string q = "")
    {
        var results = await contractService.SearchContractsAsync(q ?? "");
        return Ok(results);
    }

    [HttpGet("{contractNum}")]
    public async Task<ActionResult<ContractDetailsDto>> GetDetails(string contractNum)
    {
        if (string.IsNullOrWhiteSpace(contractNum))
            return BadRequest(new { error = "Contract number is required." });

        var details = await contractService.GetContractDetailsAsync(contractNum.Trim());

        if (details == null)
            return NotFound(new { error = $"Contract '{contractNum}' was not found." });

        return Ok(details);
    }

    [HttpGet("{contractId}/status")]
    public ActionResult<ContractStatusDto> GetStatus(string contractId)
    {
        if (string.IsNullOrWhiteSpace(contractId))
        {
            return BadRequest(new { error = "contractId is required." });
        }

        var normalized = contractId.Trim();
        var status = normalized.EndsWith("6", StringComparison.OrdinalIgnoreCase)
            ? "Pending Approval"
            : "Active";

        return Ok(new ContractStatusDto
        {
            ContractId = normalized,
            Status = status,
            Owner = "Demo Contract Team",
            Product = "Dealer Protection Plan",
            EffectiveDate = "2026-01-01",
            LastUpdated = DateTime.UtcNow,
            Source = "mock-api"
        });
    }

    [HttpGet("{contractNum}/cancellation-eligibility")]
    public ActionResult<CancellationEligibilityResult> CheckCancellationEligibility(string contractNum)
    {
        if (string.IsNullOrWhiteSpace(contractNum))
        {
            return BadRequest(new { error = "contractNum is required." });
        }

        var normalized = contractNum.Trim().ToUpperInvariant();

        // Mock logic: determine contract state based on contract number patterns
        var contractStatus = GetMockContractStatus(normalized);
        var effectiveDate = GetMockEffectiveDate(normalized);
        var expiryDate = effectiveDate.AddYears(3);
        var product = GetMockProduct(normalized);

        var result = new CancellationEligibilityResult
        {
            ContractNumber = normalized,
            Status = contractStatus,
            Product = product,
            EffectiveDate = effectiveDate.ToString("yyyy-MM-dd"),
            ExpiryDate = expiryDate.ToString("yyyy-MM-dd"),
        };

        // Cancellation rules
        if (contractStatus == "Cancelled")
        {
            result.IsEligible = false;
            result.Reason = "Contract has already been cancelled.";
            result.Summary = $"Contract {normalized} is NOT eligible for cancellation. It has already been cancelled.";
        }
        else if (contractStatus == "Expired")
        {
            result.IsEligible = false;
            result.Reason = "Contract has expired.";
            result.Summary = $"Contract {normalized} is NOT eligible for cancellation. The contract expired on {result.ExpiryDate}.";
        }
        else if (contractStatus == "Pending")
        {
            result.IsEligible = true;
            result.Reason = "Contract is pending and can be cancelled with full refund.";
            result.RefundAmount = 100.00m;
            result.RefundType = "Full Refund";
            result.Summary = $"Contract {normalized} IS eligible for cancellation. Status: Pending. A full refund will be issued.";
        }
        else // Active
        {
            var daysSinceEffective = (DateTime.Today - effectiveDate).Days;

            if (daysSinceEffective <= 30)
            {
                result.IsEligible = true;
                result.Reason = "Within 30-day free-look period. Full refund available.";
                result.RefundAmount = 100.00m;
                result.RefundType = "Full Refund";
                result.Summary = $"Contract {normalized} IS eligible for cancellation. Within the 30-day free-look period. Full refund will be issued.";
            }
            else
            {
                var totalDays = (expiryDate - effectiveDate).Days;
                var remainingDays = (expiryDate - DateTime.Today).Days;
                var proRatedPercent = Math.Round((decimal)remainingDays / totalDays * 100, 1);

                result.IsEligible = true;
                result.Reason = $"Active contract past free-look period. Pro-rated refund of {proRatedPercent}% available.";
                result.RefundAmount = proRatedPercent;
                result.RefundType = "Pro-Rated";
                result.Summary = $"Contract {normalized} IS eligible for cancellation. Pro-rated refund of {proRatedPercent}% will be calculated based on remaining coverage.";
            }
        }

        return Ok(result);
    }

    private static string GetMockContractStatus(string contractNum)
    {
        if (contractNum.EndsWith("0")) return "Cancelled";
        if (contractNum.EndsWith("9")) return "Expired";
        if (contractNum.EndsWith("8")) return "Pending";
        return "Active";
    }

    private static DateTime GetMockEffectiveDate(string contractNum)
    {
        // Contracts ending in 1-3: recent (within 30 days)
        if (contractNum.EndsWith("1") || contractNum.EndsWith("2") || contractNum.EndsWith("3"))
            return DateTime.Today.AddDays(-15);
        // Others: older contracts
        return DateTime.Today.AddDays(-180);
    }

    private static string GetMockProduct(string contractNum)
    {
        if (contractNum.StartsWith("EW") || contractNum.StartsWith("AU")) return "Extended Warranty";
        if (contractNum.StartsWith("DW")) return "Dealer Warranty";
        if (contractNum.StartsWith("GP")) return "GAP Premium";
        return "Extended Warranty";
    }
}

public class ContractStatusDto
{
    public string ContractId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = "mock-api";
}
