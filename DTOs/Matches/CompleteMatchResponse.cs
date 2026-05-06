namespace OceanClean.Api.DTOs.Matches;

public class CompleteMatchResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public ulong MatchId { get; set; }
    public string MatchCode { get; set; } = string.Empty;
}