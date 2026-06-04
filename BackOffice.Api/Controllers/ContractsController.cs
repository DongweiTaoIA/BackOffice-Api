using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
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
