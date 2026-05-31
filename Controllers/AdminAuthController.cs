using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Auth;
using OceanClean.Api.Security;

namespace OceanClean.Api.Controllers;

[ApiController]
public class AdminAuthController : ControllerBase
{
    private readonly AdminAuthService _adminAuthService;

    public AdminAuthController(AdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginResponse>> Login([FromBody] AdminLoginRequest request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = Request.Headers.UserAgent.ToString();

        var response = await _adminAuthService.LoginAsync(
            request,
            ipAddress,
            userAgent);

        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}