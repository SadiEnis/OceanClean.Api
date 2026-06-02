namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerRecentMatchDto
{
    public ulong MatchId { get; set; }
    public string MatchCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public uint DurationSeconds { get; set; }

    public uint FinalScore { get; set; }
    public uint TrashRecycledCount { get; set; }
    public uint RevivesDone { get; set; }
    public uint TimesFainted { get; set; }
    public uint EarnedCurrency { get; set; }
}