using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Admin.AuditLogs;
using OceanClean.Api.Services.Admin;
using System.Security.Claims;

namespace OceanClean.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Policy = "AdminOnly")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly AdminAuditLogsService _adminAuditLogsService;

    public AdminAuditLogsController(AdminAuditLogsService adminAuditLogsService)
    {
        _adminAuditLogsService = adminAuditLogsService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminAuditLogsListResponse>> GetAuditLogs(
        [FromQuery] AdminAuditLogsQueryRequest query)
    {
        string? adminUserIdValue = User.FindFirstValue("admin_user_id");
        string? role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("admin_role");

        if (!ulong.TryParse(adminUserIdValue, out ulong adminUserId) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        var response = await _adminAuditLogsService.GetAuditLogsAsync(
            query,
            adminUserId,
            role
        );

        return Ok(response);
    }
}