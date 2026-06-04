namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventTimelinePointDto
{
    public string Bucket { get; set; } = string.Empty;

    public long PickupTrash { get; set; }
    public long RecycleTrash { get; set; }
    public long RevivePlayer { get; set; }
    public long PlayerFainted { get; set; }
    public long UseItem { get; set; }
    public long RescueStarted { get; set; }
    public long RescueCompleted { get; set; }

    public long TotalEvents { get; set; }
}