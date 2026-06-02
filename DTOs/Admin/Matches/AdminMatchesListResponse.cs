namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchesListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }

    public List<AdminMatchListItemDto> Matches { get; set; } = new();
}