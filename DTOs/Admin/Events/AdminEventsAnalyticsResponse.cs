namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventsAnalyticsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Range { get; set; } = string.Empty;
    public string BucketType { get; set; } = string.Empty;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<AdminEventActionTypeCountDto> ActionTypeCounts { get; set; } = new();
    public List<AdminEventTimelinePointDto> Timeline { get; set; } = new();
    public List<AdminEventTopActorDto> TopActors { get; set; } = new();
}