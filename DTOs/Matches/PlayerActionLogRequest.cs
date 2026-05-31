namespace OceanClean.Api.DTOs.Matches;

public class PlayerActionLogRequest
{
    public ulong UserId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public ulong? TrashTypeId { get; set; }

    public ulong? TargetUserId { get; set; }

    public string? ItemCode { get; set; }

    public int? Value { get; set; }

    public float? PosX { get; set; }

    public float? PosY { get; set; }

    public DateTime CreatedAt { get; set; }
}