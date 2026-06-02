namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayersQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }
    public string? Status { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}