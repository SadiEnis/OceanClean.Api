namespace OceanClean.Api.DTOs.Matches;

public class MatchPlayerResultRequest
{
    public ulong UserId { get; set; }

    public uint FinalScore { get; set; }
    public uint TrashRecycledCount { get; set; }
    public uint RevivesDone { get; set; }
    public uint TimesFainted { get; set; }
    public uint EarnedCurrency { get; set; }
}