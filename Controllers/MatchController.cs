using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Matches;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly MatchService _matchService;

    public MatchController(MatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteMatch(CompleteMatchRequest request)
    {
        var response = await _matchService.CompleteMatchAsync(request);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }
}