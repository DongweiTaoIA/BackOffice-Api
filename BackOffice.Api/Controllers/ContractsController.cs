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
    [HttpGet]
    public async Task<ActionResult<ContractPagedResult>> GetContracts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? product = null,
        [FromQuery] string? dealerId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var result = await contractService.GetContractsPagedAsync(page, pageSize, search, status, product, dealerId, sortBy, sortDir);
        return Ok(result);
    }

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
    public async Task<ActionResult<ContractStatusDto>> GetStatus(string contractId)
    {
        if (string.IsNullOrWhiteSpace(contractId))
        {
            return BadRequest(new { error = "contractId is required." });
        }

        var status = await contractService.GetContractStatusAsync(contractId.Trim());

        if (status == null)
            return NotFound(new { error = $"Contract '{contractId}' was not found." });

        return Ok(status);
    }

    [HttpGet("{contractNum}/cancellation-eligibility")]
    public async Task<ActionResult<CancellationEligibilityResult>> CheckCancellationEligibility(
        string contractNum,
        [FromQuery] string? cancDt = null,
        [FromQuery] string? ruleId = null,
        [FromQuery] string? cancType = null,
        [FromQuery] string? lang = null,
        [FromQuery] string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(contractNum))
        {
            return BadRequest(new { error = "contractNum is required." });
        }

        DateTime cancellationDate;
        if (string.IsNullOrWhiteSpace(cancDt))
        {
            cancellationDate = DateTime.Today;
        }
        else if (!DateTime.TryParse(cancDt, out cancellationDate))
        {
            return BadRequest(new { error = $"cancDt '{cancDt}' is not a valid date." });
        }

        var result = await contractService.CheckCancellationEligibilityAsync(
            contractNum.Trim(),
            cancellationDate,
            ruleId,
            cancType,
            lang,
            userId);

        if (result == null)
            return NotFound(new { error = $"Contract '{contractNum}' was not found." });

        return Ok(result);
    }
}
