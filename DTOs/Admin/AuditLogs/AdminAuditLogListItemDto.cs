namespace OceanClean.Api.DTOs.Admin.AuditLogs;

public class AdminAuditLogListItemDto
{
    public ulong AuditLogId { get; set; }

    public ulong AdminUserId { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminRole { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public ulong? TargetId { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
}