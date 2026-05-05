using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly PlayerProfileService _profileService;
    public PlayerController(PlayerProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("profile/{userId:long}")]
    public async Task<IActionResult> GetProfile(ulong userId)
    {
        var response = await _profileService.GetProfileAsync(userId);
        
        if (response == null)
            return NotFound(
                new
                {
                    success = false,
                    message = "Profile not found"
                });
        return Ok(response);
    }
}