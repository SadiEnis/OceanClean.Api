namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventLogItemDto
{
    public ulong LogId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string ActionType { get; set; } = string.Empty;

    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ulong MatchId { get; set; }
    public string MatchCode { get; set; } = string.Empty;

    public ulong? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string? TargetDisplayName { get; set; }

    public ulong? ShopItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemType { get; set; }

    public ulong? TrashTypeId { get; set; }
    public string? TrashTypeCode { get; set; }
    public string? TrashTypeName { get; set; }

    public int? Value { get; set; }

    public float? PosX { get; set; }
    public float? PosY { get; set; }
}