namespace OceanClean.Api.DTOs.Admin.AuditLogs;

public class AdminAuditLogsListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }

    public List<AdminAuditLogListItemDto> AuditLogs { get; set; } = new();
}