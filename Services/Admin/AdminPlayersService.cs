using OceanClean.Api.DTOs.Admin.Players;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

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

    public async Task<AdminPlayerDetailResponse> GetPlayerDetailAsync(ulong userId)
    {
        if (userId == 0)
        {
            return new AdminPlayerDetailResponse
            {
                Success = false,
                Message = "UserId must be greater than zero."
            };
        }

        var player = await _adminPlayersRepository.GetPlayerDetailAsync(userId);

        if (player == null)
        {
            return new AdminPlayerDetailResponse
            {
                Success = false,
                Message = "Player not found."
            };
        }

        player.Inventory = await _adminPlayersRepository.GetPlayerInventoryAsync(userId);
        player.RecentMatches = await _adminPlayersRepository.GetPlayerRecentMatchesAsync(userId);
        player.RecentPurchases = await _adminPlayersRepository.GetPlayerRecentPurchasesAsync(userId);
        player.RecentActionLogs = await _adminPlayersRepository.GetPlayerRecentActionLogsAsync(userId);

        return new AdminPlayerDetailResponse
        {
            Success = true,
            Message = "Player detail retrieved successfully.",
            Player = player
        };
    }

    public async Task<AdminUpdatePlayerStatusResponse> UpdatePlayerStatusAsync(
        ulong userId,
        AdminUpdatePlayerStatusRequest request,
        ulong adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        if (userId == 0)
        {
            return new AdminUpdatePlayerStatusResponse
            {
                Success = false,
                Message = "UserId must be greater than zero.",
                UserId = userId
            };
        }

        if (adminUserId == 0)
        {
            return new AdminUpdatePlayerStatusResponse
            {
                Success = false,
                Message = "Admin user id is missing.",
                UserId = userId
            };
        }

        string newStatus = request.NewStatus.Trim().ToLowerInvariant();

        if (!IsValidPlayerStatus(newStatus))
        {
            return new AdminUpdatePlayerStatusResponse
            {
                Success = false,
                Message = "Invalid player status. Allowed values: active, inactive, banned.",
                UserId = userId
            };
        }

        return await _adminPlayersRepository.UpdatePlayerStatusAsync(
            userId,
            newStatus,
            adminUserId,
            ipAddress,
            userAgent
        );
    }

    public async Task<AdminPlayerItemTimeseriesResponse> GetPlayerItemTimeseriesAsync(
        ulong userId,
        AdminPlayerItemTimeseriesQueryRequest query)
    {
        if (userId == 0)
        {
            return new AdminPlayerItemTimeseriesResponse
            {
                Success = false,
                Message = "UserId must be greater than zero."
            };
        }

        NormalizeItemTimeseriesQuery(query);

        var (from, to) = ResolveDateRange(query);
        string bucketType = ResolveBucketType(query.Range);

        var points = await _adminPlayersRepository.GetPlayerItemTimeseriesAsync(
            userId,
            from,
            to,
            bucketType
        );

        return new AdminPlayerItemTimeseriesResponse
        {
            Success = true,
            Message = "Player item timeseries retrieved successfully.",
            Range = query.Range,
            BucketType = bucketType,
            From = from,
            To = to,
            Points = points
        };
    }

    private static void NormalizeItemTimeseriesQuery(AdminPlayerItemTimeseriesQueryRequest query)
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
        AdminPlayerItemTimeseriesQueryRequest query)
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


    private static bool IsValidPlayerStatus(string status)
    {
        return status is "active" or "inactive" or "banned";
    }
}