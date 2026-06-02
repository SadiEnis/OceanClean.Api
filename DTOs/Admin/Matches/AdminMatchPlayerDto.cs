namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchPlayerDto
{
    public ulong UserId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public uint FinalScore { get; set; }
    public uint TrashRecycledCount { get; set; }
    public uint RevivesDone { get; set; }
    public uint TimesFainted { get; set; }
    public uint EarnedCurrency { get; set; }
}