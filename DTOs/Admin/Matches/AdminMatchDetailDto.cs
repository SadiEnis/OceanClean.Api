namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchDetailDto
{
    public ulong MatchId { get; set; }
    public ulong? LobbyId { get; set; }

    public string MatchCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public uint DurationSeconds { get; set; }

    public uint TotalTrashSpawned { get; set; }
    public uint TotalTrashRecycled { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<AdminMatchPlayerDto> Players { get; set; } = new();
    public List<AdminMatchUsedItemDto> UsedItems { get; set; } = new();
    public List<AdminMatchRescueEventDto> RescueEvents { get; set; } = new();
    public List<AdminMatchFaintReviveEventDto> FaintReviveEvents { get; set; } = new();
}