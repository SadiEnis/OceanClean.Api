using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Leaderboard;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderboardService _leaderboardService;

    public LeaderboardController(LeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard(
        [FromQuery] int limit = 10)
    {
        var response = await _leaderboardService.GetLeaderboardAsync(limit);
        return Ok(response);
    }
}