using OceanClean.Api.DTOs.Admin.Events;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

public class AdminEventsService
{
    private readonly AdminEventsRepository _adminEventsRepository;

    public AdminEventsService(AdminEventsRepository adminEventsRepository)
    {
        _adminEventsRepository = adminEventsRepository;
    }

    public async Task<AdminEventsListResponse> GetEventsAsync(AdminEventsQueryRequest query)
    {
        NormalizeQuery(query);

        long totalCount = await _adminEventsRepository.GetEventsCountAsync(query);
        var events = await _adminEventsRepository.GetEventsAsync(query);

        return new AdminEventsListResponse
        {
            Success = true,
            Message = "Events retrieved successfully.",
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Events = events
        };
    }

    private static void NormalizeQuery(AdminEventsQueryRequest query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        query.Username = string.IsNullOrWhiteSpace(query.Username)
            ? null
            : query.Username.Trim();

        query.MatchCode = string.IsNullOrWhiteSpace(query.MatchCode)
            ? null
            : query.MatchCode.Trim();

        query.ActionType = string.IsNullOrWhiteSpace(query.ActionType)
            ? null
            : query.ActionType.Trim();

        query.SortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "createdAt"
            : query.SortBy.Trim();

        query.SortDirection = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
        {
            (query.From, query.To) = (query.To, query.From);
        }
    }
}