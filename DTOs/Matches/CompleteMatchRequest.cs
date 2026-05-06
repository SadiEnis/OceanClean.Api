namespace OceanClean.Api.DTOs.Matches;

public class CompleteMatchRequest
{
    public string MatchCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }

    public uint DurationSeconds { get; set; }

    public uint TotalTrashSpawned { get; set; }
    public uint TotalTrashRecycled { get; set; }

    public List<MatchPlayerResultRequest> Players { get; set; } = new();
}