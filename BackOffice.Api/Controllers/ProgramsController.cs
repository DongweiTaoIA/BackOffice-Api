using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/programs")]
[Authorize]
public class ProgramsController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Look up programs by code or name (English/French). Returns each match
    /// with its parent product (resolved via cfContractGroup) so callers can
    /// disambiguate program names that exist under multiple products.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<ProgramLookupResult>>> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<ProgramLookupResult>());

        var results = await productService.SearchProgramsAsync(q);
        return Ok(results);
    }
}
