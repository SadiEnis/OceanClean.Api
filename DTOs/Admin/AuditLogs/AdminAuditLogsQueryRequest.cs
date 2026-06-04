namespace OceanClean.Api.DTOs.Admin.AuditLogs;

public class AdminAuditLogsQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? ActionType { get; set; }
    public string? TargetType { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}