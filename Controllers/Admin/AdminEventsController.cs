using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Events;
using OceanClean.Api.Services.Admin;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/events")]
[Authorize(Policy = "AdminOnly")]
public class AdminEventsController : ControllerBase
{
    private readonly AdminEventsService _adminEventsService;

    public AdminEventsController(AdminEventsService adminEventsService)
    {
        _adminEventsService = adminEventsService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminEventsListResponse>> GetEvents(
        [FromQuery] AdminEventsQueryRequest query)
    {
        var response = await _adminEventsService.GetEventsAsync(query);
        return Ok(response);
    }
    
    [HttpGet("analytics")]
    public async Task<ActionResult<AdminEventsAnalyticsResponse>> GetAnalytics(
        [FromQuery] AdminEventsAnalyticsQueryRequest query)
    {
        var response = await _adminEventsService.GetAnalyticsAsync(query);
        return Ok(response);
    }
}