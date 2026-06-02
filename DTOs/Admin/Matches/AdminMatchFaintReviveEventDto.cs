namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchFaintReviveEventDto
{
    public ulong LogId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ulong? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string? TargetDisplayName { get; set; }

    public int? Value { get; set; }

    public float? PosX { get; set; }
    public float? PosY { get; set; }

    public DateTime CreatedAt { get; set; }
}