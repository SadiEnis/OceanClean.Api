using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.Auth;
using OceanClean.Api.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AdminRefreshTokenResponse>> Refresh(
        [FromBody] AdminRefreshTokenRequest request)
    {
        var response = await _adminAuthService.RefreshAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult<AdminLogoutResponse>> Logout(
        [FromBody] AdminLogoutRequest request)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = Request.Headers.UserAgent.ToString();

        var response = await _adminAuthService.LogoutAsync(
            request,
            ipAddress,
            userAgent
        );

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("me")]
    public ActionResult<AdminUserDto> Me()
    {
        string? adminUserIdValue = User.FindFirstValue("admin_user_id");
        string? username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
        string? role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("admin_role");

        if (!ulong.TryParse(adminUserIdValue, out ulong adminUserId))
        {
            return Unauthorized();
        }

        return Ok(new AdminUserDto
        {
            AdminUserId = adminUserId,
            Username = username ?? string.Empty,
            Role = role ?? string.Empty
        });
    }
}