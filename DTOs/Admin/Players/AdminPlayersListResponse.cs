namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayersListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }

    public List<AdminPlayerListItemDto> Players { get; set; } = new();
}