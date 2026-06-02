using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Matches;
using OceanClean.Api.Services.Admin;

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
    
    [HttpGet("{matchId:long}")]
    public async Task<ActionResult<AdminMatchDetailResponse>> GetMatchDetail(long matchId)
    {
        if (matchId <= 0)
        {
            return BadRequest(new AdminMatchDetailResponse
            {
                Success = false,
                Message = "MatchId must be greater than zero."
            });
        }

        var response = await _adminMatchesService.GetMatchDetailAsync((ulong)matchId);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}