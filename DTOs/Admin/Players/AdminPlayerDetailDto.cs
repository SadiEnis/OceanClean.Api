namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerDetailDto
{
    public ulong UserId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PlayerStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public uint SoftCurrency { get; set; }
    public uint TotalScore { get; set; }
    public uint TotalMatchesPlayed { get; set; }
    public uint TotalMatchesWon { get; set; }
    public uint TotalTrashRecycled { get; set; }
    public uint TotalRevivesDone { get; set; }
    public uint TotalTimesFainted { get; set; }
    public uint TotalPlaytimeSeconds { get; set; }

    public List<AdminPlayerInventoryItemDto> Inventory { get; set; } = new();
    public List<AdminPlayerRecentMatchDto> RecentMatches { get; set; } = new();
}