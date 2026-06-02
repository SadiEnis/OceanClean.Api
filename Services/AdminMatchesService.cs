using OceanClean.Api.DTOs.Admin.Matches;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class AdminMatchesService
{
    private readonly AdminMatchesRepository _adminMatchesRepository;

    public AdminMatchesService(AdminMatchesRepository adminMatchesRepository)
    {
        _adminMatchesRepository = adminMatchesRepository;
    }

    public async Task<AdminMatchesListResponse> GetMatchesAsync(AdminMatchesQueryRequest query)
    {
        NormalizeQuery(query);

        long totalCount = await _adminMatchesRepository.GetMatchesCountAsync(query);
        var records = await _adminMatchesRepository.GetMatchesAsync(query);

        return new AdminMatchesListResponse
        {
            Success = true,
            Message = "Matches retrieved successfully.",
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Matches = records.Select(record => new AdminMatchListItemDto
            {
                MatchId = record.MatchId,
                MatchCode = record.MatchCode,
                StartedAt = record.StartedAt,
                EndedAt = record.EndedAt,
                DurationSeconds = record.DurationSeconds,
                TotalTrashSpawned = record.TotalTrashSpawned,
                TotalTrashRecycled = record.TotalTrashRecycled,
                PlayerCount = record.PlayerCount,
                TotalScore = record.TotalScore,
                TotalCurrencyEarned = record.TotalCurrencyEarned,
                CreatedAt = record.CreatedAt
            }).ToList()
        };
    }

    private static void NormalizeQuery(AdminMatchesQueryRequest query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        query.Search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        query.SortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "startedAt"
            : query.SortBy.Trim();

        query.SortDirection = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        if (query.StartedFrom.HasValue && query.StartedTo.HasValue)
        {
            if (query.StartedFrom.Value > query.StartedTo.Value)
            {
                (query.StartedFrom, query.StartedTo) = (query.StartedTo, query.StartedFrom);
            }
        }
    }
}