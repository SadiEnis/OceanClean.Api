namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerItemTimeseriesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Range { get; set; } = string.Empty;
    public string BucketType { get; set; } = string.Empty;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<AdminPlayerItemTimeseriesPointDto> Points { get; set; } = new();
}