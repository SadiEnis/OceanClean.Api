namespace OceanClean.Api.DTOs.Leaderboard;

public class LeaderboardResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public List<LeaderboardEntryDto> Players { get; set; } = new();
}