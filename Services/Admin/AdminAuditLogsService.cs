using OceanClean.Api.DTOs.Admin.AuditLogs;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

public class AdminAuditLogsService
{
    private readonly AdminAuditLogsRepository _adminAuditLogsRepository;

    public AdminAuditLogsService(AdminAuditLogsRepository adminAuditLogsRepository)
    {
        _adminAuditLogsRepository = adminAuditLogsRepository;
    }

    public async Task<AdminAuditLogsListResponse> GetAuditLogsAsync(
        AdminAuditLogsQueryRequest query,
        ulong currentAdminUserId,
        string currentAdminRole)
    {
        NormalizeQuery(query);

        var (items, totalCount) = await _adminAuditLogsRepository.GetAuditLogsAsync(
            query,
            currentAdminUserId,
            currentAdminRole
        );

        return new AdminAuditLogsListResponse
        {
            Success = true,
            Message = "Admin audit logs retrieved successfully.",
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            AuditLogs = items
        };
    }

    private static void NormalizeQuery(AdminAuditLogsQueryRequest query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        query.Search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        query.ActionType = string.IsNullOrWhiteSpace(query.ActionType)
            ? null
            : query.ActionType.Trim();

        query.TargetType = string.IsNullOrWhiteSpace(query.TargetType)
            ? null
            : query.TargetType.Trim();

        query.SortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "createdAt"
            : query.SortBy.Trim();

        query.SortDirection = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
        {
            (query.From, query.To) = (query.To, query.From);
        }
    }
}