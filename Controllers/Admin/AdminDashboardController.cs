using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Dashboard;
using OceanClean.Api.Services.Admin;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly AdminDashboardService _adminDashboardService;

    public AdminDashboardController(AdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AdminDashboardSummaryResponse>> GetSummary()
    {
        var response = await _adminDashboardService.GetSummaryAsync();
        return Ok(response);
    }
}