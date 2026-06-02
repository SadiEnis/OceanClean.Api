namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventsListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }

    public List<AdminEventLogItemDto> Events { get; set; } = new();
}