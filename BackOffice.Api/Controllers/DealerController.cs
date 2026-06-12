using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/dealers")]
[Authorize]
public class DealerController(IDealerService dealerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DealerPagedResult>> GetDealers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? province = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var result = await dealerService.GetDealersPagedAsync(page, pageSize, search, status, province, sortBy, sortDir);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<DealerSearchResult>>> Search([FromQuery] string q = "")
    {
        var results = await dealerService.SearchDealersAsync(q ?? "");
        return Ok(results);
    }

    [HttpGet("{dealerCode}")]
    public async Task<ActionResult<DealerDetailsDto>> GetDetails(string dealerCode)
    {
        if (string.IsNullOrWhiteSpace(dealerCode))
            return BadRequest(new { error = "Dealer code is required." });

        var details = await dealerService.GetDealerDetailsAsync(dealerCode.Trim());

        if (details == null)
            return NotFound(new { error = $"Dealer '{dealerCode}' was not found." });

        return Ok(details);
    }
}
