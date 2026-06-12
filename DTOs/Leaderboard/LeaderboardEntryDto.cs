namespace OceanClean.Api.DTOs.Leaderboard;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public ulong UserId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public int TotalScore { get; set; }
    public int TotalMatchesPlayed { get; set; }
    public int TotalTrashRecycled { get; set; }
}