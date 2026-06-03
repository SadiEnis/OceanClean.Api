using OceanClean.Api.DTOs.Admin.Dashboard;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

public class AdminDashboardService
{
    private readonly AdminDashboardRepository _adminDashboardRepository;

    public AdminDashboardService(AdminDashboardRepository adminDashboardRepository)
    {
        _adminDashboardRepository = adminDashboardRepository;
    }

    public async Task<AdminDashboardSummaryResponse> GetSummaryAsync()
    {
        var summary = await _adminDashboardRepository.GetSummaryAsync();

        return new AdminDashboardSummaryResponse
        {
            Success = true,
            Message = "Dashboard summary retrieved successfully.",
            Summary = summary
        };
    }
    
    public async Task<AdminDashboardActivityResponse> GetActivityAsync(
        AdminDashboardActivityQueryRequest query)
    {
        NormalizeActivityQuery(query);

        var (from, to) = ResolveDateRange(query);
        string bucketType = ResolveBucketType(query.Range);

        var points = await _adminDashboardRepository.GetActivityAsync(
            from,
            to,
            bucketType
        );

        return new AdminDashboardActivityResponse
        {
            Success = true,
            Message = "Dashboard activity retrieved successfully.",
            Range = query.Range,
            BucketType = bucketType,
            From = from,
            To = to,
            Points = points
        };
    }

    private static void NormalizeActivityQuery(AdminDashboardActivityQueryRequest query)
    {
        query.Range = string.IsNullOrWhiteSpace(query.Range)
            ? "weekly"
            : query.Range.Trim();

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
        {
            (query.From, query.To) = (query.To, query.From);
        }
    }

    private static (DateTime? From, DateTime? To) ResolveDateRange(
        AdminDashboardActivityQueryRequest query)
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
            _ => (now.Date.AddDays(-7), now)
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