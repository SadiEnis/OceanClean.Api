using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Players;
using OceanClean.Api.Services.Admin;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/players")]
[Authorize(Policy = "AdminOnly")]
public class AdminPlayersController : ControllerBase
{
    private readonly AdminPlayersService _adminPlayersService;

    public AdminPlayersController(AdminPlayersService adminPlayersService)
    {
        _adminPlayersService = adminPlayersService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminPlayersListResponse>> GetPlayers(
        [FromQuery] AdminPlayersQueryRequest query)
    {
        var response = await _adminPlayersService.GetPlayersAsync(query);
        return Ok(response);
    }
    
    [HttpGet("{userId:long}")]
    public async Task<ActionResult<AdminPlayerDetailResponse>> GetPlayerDetail(long userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new AdminPlayerDetailResponse
            {
                Success = false,
                Message = "UserId must be greater than zero."
            });
        }

        var response = await _adminPlayersService.GetPlayerDetailAsync((ulong)userId);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
    
    [Authorize(Policy = "ModeratorOrAbove")]
    [HttpPatch("{userId:long}/status")]
    public async Task<ActionResult<AdminUpdatePlayerStatusResponse>> UpdatePlayerStatus(
        long userId,
        [FromBody] AdminUpdatePlayerStatusRequest request)
    {
        if (userId <= 0)
        {
            return BadRequest(new AdminUpdatePlayerStatusResponse
            {
                Success = false,
                Message = "UserId must be greater than zero."
            });
        }

        string? adminUserIdValue = User.FindFirstValue("admin_user_id");

        if (!ulong.TryParse(adminUserIdValue, out ulong adminUserId))
        {
            return Unauthorized();
        }

        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = Request.Headers.UserAgent.ToString();

        var response = await _adminPlayersService.UpdatePlayerStatusAsync(
            (ulong)userId,
            request,
            adminUserId,
            ipAddress,
            userAgent
        );

        if (!response.Success)
        {
            if (response.Message == "Player not found.")
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpGet("{userId:long}/item-timeseries")]
    public async Task<ActionResult<AdminPlayerItemTimeseriesResponse>> GetPlayerItemTimeseries(
        long userId,
        [FromQuery] AdminPlayerItemTimeseriesQueryRequest query)
    {
        if (userId <= 0)
        {
            return BadRequest(new AdminPlayerItemTimeseriesResponse
            {
                Success = false,
                Message = "UserId must be greater than zero."
            });
        }

        var response = await _adminPlayersService.GetPlayerItemTimeseriesAsync(
            (ulong)userId,
            query
        );

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}