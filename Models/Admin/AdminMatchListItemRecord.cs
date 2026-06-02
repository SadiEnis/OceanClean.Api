namespace OceanClean.Api.Models.Admin;

public class AdminMatchListItemRecord
{
    public ulong MatchId { get; set; }
    public string MatchCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public uint DurationSeconds { get; set; }

    public uint TotalTrashSpawned { get; set; }
    public uint TotalTrashRecycled { get; set; }

    public uint PlayerCount { get; set; }
    public uint TotalScore { get; set; }
    public uint TotalCurrencyEarned { get; set; }

    public DateTime CreatedAt { get; set; }
}