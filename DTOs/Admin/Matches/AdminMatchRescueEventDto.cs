namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchRescueEventDto
{
    public ulong LogId { get; set; }

    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public int? ScoreReward { get; set; }

    public float? PosX { get; set; }
    public float? PosY { get; set; }

    public DateTime CreatedAt { get; set; }
}