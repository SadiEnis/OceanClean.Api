namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerItemTimeseriesQueryRequest
{
    public string Range { get; set; } = "weekly";

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}