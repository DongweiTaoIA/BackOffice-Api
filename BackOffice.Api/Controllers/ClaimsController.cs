using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ClaimPagedResult>> GetClaims(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? claimType = null,
        [FromQuery] string? dealerId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var result = await claimService.GetClaimsPagedAsync(page, pageSize, search, status, claimType, dealerId, sortBy, sortDir);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ClaimSearchResult>>> Search([FromQuery] string q = "")
    {
        var results = await claimService.SearchClaimsAsync(q ?? "");
        return Ok(results);
    }

    [HttpGet("{claimNum}")]
    public async Task<ActionResult<ClaimDetailsDto>> GetDetails(string claimNum)
    {
        if (string.IsNullOrWhiteSpace(claimNum))
            return BadRequest(new { error = "Claim number is required." });

        var details = await claimService.GetClaimDetailsAsync(claimNum.Trim());

        if (details == null)
            return NotFound(new { error = $"Claim '{claimNum}' was not found." });

        return Ok(details);
    }
}
