namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventsAnalyticsQueryRequest
{
    public string Range { get; set; } = "weekly";

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}