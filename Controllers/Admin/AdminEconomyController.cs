using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Economy;
using OceanClean.Api.Services.Admin;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/economy")]
[Authorize(Policy = "AdminOnly")]
public class AdminEconomyController : ControllerBase
{
    private readonly AdminEconomyService _adminEconomyService;

    public AdminEconomyController(AdminEconomyService adminEconomyService)
    {
        _adminEconomyService = adminEconomyService;
    }

    [HttpGet("item-summary")]
    public async Task<ActionResult<AdminEconomyItemSummaryResponse>> GetItemSummary(
        [FromQuery] AdminEconomyQueryRequest query)
    {
        var response = await _adminEconomyService.GetItemSummaryAsync(query);
        return Ok(response);
    }
    
    [HttpGet("item-timeseries")]
    public async Task<ActionResult<AdminEconomyItemTimeseriesResponse>> GetItemTimeseries(
        [FromQuery] AdminEconomyQueryRequest query)
    {
        var response = await _adminEconomyService.GetItemTimeseriesAsync(query);
        return Ok(response);
    }
}