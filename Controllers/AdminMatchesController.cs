using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Matches;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/matches")]
[Authorize(Policy = "AdminOnly")]
public class AdminMatchesController : ControllerBase
{
    private readonly AdminMatchesService _adminMatchesService;

    public AdminMatchesController(AdminMatchesService adminMatchesService)
    {
        _adminMatchesService = adminMatchesService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminMatchesListResponse>> GetMatches(
        [FromQuery] AdminMatchesQueryRequest query)
    {
        var response = await _adminMatchesService.GetMatchesAsync(query);
        return Ok(response);
    }
}