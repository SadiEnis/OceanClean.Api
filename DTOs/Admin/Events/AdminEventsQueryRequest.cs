namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventsQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public string? Username { get; set; }
    public string? MatchCode { get; set; }
    public string? ActionType { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}