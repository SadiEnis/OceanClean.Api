using OceanClean.Api.DTOs.Admin.Players;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class AdminPlayersService
{
    private readonly AdminPlayersRepository _adminPlayersRepository;

    public AdminPlayersService(AdminPlayersRepository adminPlayersRepository)
    {
        _adminPlayersRepository = adminPlayersRepository;
    }

    public async Task<AdminPlayersListResponse> GetPlayersAsync(AdminPlayersQueryRequest query)
    {
        NormalizeQuery(query);

        var totalCount = await _adminPlayersRepository.GetPlayersCountAsync(query);
        var records = await _adminPlayersRepository.GetPlayersAsync(query);

        return new AdminPlayersListResponse
        {
            Success = true,
            Message = "Players retrieved successfully.",
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Players = records.Select(record => new AdminPlayerListItemDto
            {
                UserId = record.UserId,
                Username = record.Username,
                DisplayName = record.DisplayName,
                PlayerStatus = record.PlayerStatus,
                TotalPoints = record.TotalPoints,
                TotalMatches = record.TotalMatches,
                SoftCurrency = record.SoftCurrency,
                CreatedAt = record.CreatedAt,
                LastLogin = record.LastLogin
            }).ToList()
        };
    }

    private static void NormalizeQuery(AdminPlayersQueryRequest query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        query.Search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        query.Status = string.IsNullOrWhiteSpace(query.Status)
            ? null
            : query.Status.Trim();

        query.SortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "createdAt"
            : query.SortBy.Trim();

        query.SortDirection = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";
    }
}