namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventActionTypeCountDto
{
    public string ActionType { get; set; } = string.Empty;
    public long EventCount { get; set; }
}