using BackOffice.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("active-dealers")]
    public async Task<ActionResult> GetActiveDealers()
    {
        var count = await dashboardService.GetTotalActiveDealersAsync();
        return Ok(new { count });
    }

    [HttpGet("products")]
    public async Task<ActionResult> GetProducts()
    {
        var count = await dashboardService.GetTotalProductsAsync();
        return Ok(new { count });
    }

    [HttpGet("active-programs")]
    public async Task<ActionResult> GetActivePrograms()
    {
        var count = await dashboardService.GetTotalActiveProgramsAsync();
        return Ok(new { count });
    }

    [HttpGet("active-contracts")]
    public async Task<ActionResult> GetActiveContracts()
    {
        var count = await dashboardService.GetTotalActiveContractsAsync();
        return Ok(new { count });
    }

    [HttpGet("active-customers")]
    public async Task<ActionResult> GetActiveCustomers()
    {
        var count = await dashboardService.GetTotalActiveCustomersAsync();
        return Ok(new { count });
    }

    [HttpGet("active-vehicles")]
    public async Task<ActionResult> GetActiveVehicles()
    {
        var count = await dashboardService.GetTotalActiveVehiclesAsync();
        return Ok(new { count });
    }
}
