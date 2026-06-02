namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchesQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public DateTime? StartedFrom { get; set; }
    public DateTime? StartedTo { get; set; }

    public string SortBy { get; set; } = "startedAt";
    public string SortDirection { get; set; } = "desc";
}