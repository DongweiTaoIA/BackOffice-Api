using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/eligibility")]
[Authorize]
public class EligibilityController(IDealerService dealerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<EligibilityResult>> Check(
        [FromQuery] string? dealerId,
        [FromQuery] string? dealerName,
        [FromQuery] string? productId,
        [FromQuery] string? productName,
        [FromQuery] string? programId,
        [FromQuery] string? programName)
    {
        if (string.IsNullOrWhiteSpace(dealerId) && string.IsNullOrWhiteSpace(dealerName))
        {
            return BadRequest(new { error = "At least one dealer identifier (dealerId or dealerName) is required." });
        }

        var query = new EligibilityQuery
        {
            DealerId = dealerId,
            DealerName = dealerName,
            ProductId = productId,
            ProductName = productName,
            ProgramId = programId,
            ProgramName = programName,
        };

        var result = await dealerService.CheckEligibilityAsync(query);
        return Ok(result);
    }

    [HttpGet("max-markup")]
    public async Task<ActionResult<MaxMarkupResult>> GetMaxMarkup([FromQuery] string? programId, [FromQuery] string? programName)
    {
        var identifier = programId ?? programName;
        if (string.IsNullOrWhiteSpace(identifier))
            return BadRequest(new { error = "Either programId or programName is required." });

        var result = await dealerService.GetMaxMarkupAsync(identifier.Trim());
        return Ok(result);
    }

    [HttpPost("activate")]
    public async Task<ActionResult<ProgramActionResult>> Activate([FromBody] ActivateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DealerId))
            return BadRequest(new { error = "dealerId is required." });
        if (string.IsNullOrWhiteSpace(request.ProgramId))
            return BadRequest(new { error = "programId is required." });

        var result = await dealerService.ActivateProgramAsync(request);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("deactivate")]
    public async Task<ActionResult<ProgramActionResult>> Deactivate([FromBody] DeactivateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DealerId))
            return BadRequest(new { error = "dealerId is required." });
        if (string.IsNullOrWhiteSpace(request.ProgramId))
            return BadRequest(new { error = "programId is required." });
        if (string.IsNullOrWhiteSpace(request.ExpiryDate))
            return BadRequest(new { error = "expiryDate is required." });

        var result = await dealerService.DeactivateProgramAsync(request);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
