namespace OceanClean.Api.Models.Player;

public class PlayerProfileRecord
{
    public ulong UserId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public uint SoftCurrency { get; set; }
    public uint TotalScore { get; set; }

    public uint TotalMatchesPlayed { get; set; }
    public uint TotalMatchesWon { get; set; }

    public uint TotalTrashRecycled { get; set; }

    public uint TotalRevivesDone { get; set; }
    public uint TotalTimesFainted { get; set; }

    public uint TotalPlaytimeSeconds { get; set; }
}