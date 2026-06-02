namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerRecentActionLogDto
{
    public ulong LogId { get; set; }

    public ulong MatchId { get; set; }
    public string MatchCode { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public ulong? ShopItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }

    public ulong? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string? TargetDisplayName { get; set; }

    public ulong? TrashTypeId { get; set; }
    public string? TrashTypeCode { get; set; }
    public string? TrashTypeName { get; set; }

    public int? Value { get; set; }
    public float? PosX { get; set; }
    public float? PosY { get; set; }

    public DateTime CreatedAt { get; set; }
}