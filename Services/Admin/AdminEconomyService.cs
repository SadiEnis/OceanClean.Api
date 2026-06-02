using OceanClean.Api.DTOs.Admin.Economy;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

public class AdminEconomyService
{
    private readonly AdminEconomyRepository _adminEconomyRepository;

    public AdminEconomyService(AdminEconomyRepository adminEconomyRepository)
    {
        _adminEconomyRepository = adminEconomyRepository;
    }

    public async Task<AdminEconomyItemSummaryResponse> GetItemSummaryAsync(
        AdminEconomyQueryRequest query)
    {
        NormalizeQuery(query);

        var (from, to) = ResolveDateRange(query);

        var topPurchasedItems = await _adminEconomyRepository.GetTopPurchasedItemsAsync(
            from,
            to,
            query.Limit
        );

        var topUsedItems = await _adminEconomyRepository.GetTopUsedItemsAsync(
            from,
            to,
            query.Limit
        );

        return new AdminEconomyItemSummaryResponse
        {
            Success = true,
            Message = "Economy item summary retrieved successfully.",
            Range = query.Range,
            From = from,
            To = to,
            TopPurchasedItems = topPurchasedItems,
            TopUsedItems = topUsedItems
        };
    }

    private static void NormalizeQuery(AdminEconomyQueryRequest query)
    {
        query.Range = string.IsNullOrWhiteSpace(query.Range)
            ? "all"
            : query.Range.Trim();

        query.Limit = Math.Clamp(query.Limit, 1, 50);

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
        {
            (query.From, query.To) = (query.To, query.From);
        }
    }

    private static (DateTime? From, DateTime? To) ResolveDateRange(
        AdminEconomyQueryRequest query)
    {
        DateTime now = DateTime.UtcNow;

        return query.Range switch
        {
            "daily" => (now.Date, now),
            "weekly" => (now.Date.AddDays(-7), now),
            "monthly" => (now.Date.AddMonths(-1), now),
            "sixMonths" => (now.Date.AddMonths(-6), now),
            "custom" => (query.From, query.To),
            "all" => (null, null),
            _ => (null, null)
        };
    }
    
    public async Task<AdminEconomyItemTimeseriesResponse> GetItemTimeseriesAsync(
        AdminEconomyQueryRequest query)
    {
        NormalizeQuery(query);

        var (from, to) = ResolveDateRange(query);
        string bucketType = ResolveBucketType(query.Range);

        var points = await _adminEconomyRepository.GetItemTimeseriesAsync(
            from,
            to,
            bucketType
        );

        return new AdminEconomyItemTimeseriesResponse
        {
            Success = true,
            Message = "Economy item timeseries retrieved successfully.",
            Range = query.Range,
            BucketType = bucketType,
            From = from,
            To = to,
            Points = points
        };
    }
    
    private static string ResolveBucketType(string range)
    {
        return range switch
        {
            "daily" => "hour",
            "weekly" => "day",
            "monthly" => "day",
            "sixMonths" => "month",
            "all" => "month",
            "custom" => "day",
            _ => "day"
        };
    }
}