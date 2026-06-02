using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Players;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

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
}