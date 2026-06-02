using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.Players;
using OceanClean.Api.Models.Admin;

namespace OceanClean.Api.Repositories;

public class AdminPlayersRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminPlayersRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> GetPlayersCountAsync(AdminPlayersQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        var whereClauses = BuildWhereClauses(query, command);

        command.CommandText = $"""
                               SELECT COUNT(*)
                               FROM users u
                               JOIN player_profiles pp ON pp.user_id = u.user_id
                               {whereClauses};
                               """;

        object? result = await command.ExecuteScalarAsync();

        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt64(result);
    }

    public async Task<List<AdminPlayerListItemRecord>> GetPlayersAsync(AdminPlayersQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        var whereClauses = BuildWhereClauses(query, command);
        var orderBy = BuildOrderBy(query);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, 100);
        int offset = (page - 1) * pageSize;

        command.CommandText = $"""
                               SELECT
                                   u.user_id,
                                   u.username,
                                   u.player_status,
                                   u.created_at,
                                   u.display_name,
                                   pp.total_score,
                                   pp.total_matches_played,
                                   pp.soft_currency,
                                   u.last_login_at
                               FROM users u
                               JOIN player_profiles pp ON pp.user_id = u.user_id
                               {whereClauses}
                               {orderBy}
                               LIMIT @pageSize OFFSET @offset;
                               """;

        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@offset", offset);

        var players = new List<AdminPlayerListItemRecord>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            players.Add(new AdminPlayerListItemRecord
            {
                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                PlayerStatus = reader.GetString("player_status"),
                CreatedAt = reader.GetDateTime("created_at"),

                DisplayName = reader.GetString("display_name"),
                TotalPoints = reader.GetUInt32("total_score"),
                TotalMatches = reader.GetUInt32("total_matches_played"),
                SoftCurrency = reader.GetUInt32("soft_currency"),
                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login_at"))
                    ? null
                    : reader.GetDateTime("last_login_at")
            });
        }

        return players;
    }

    private static string BuildWhereClauses(
        AdminPlayersQueryRequest query,
        MySqlCommand command)
    {
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            where.Add("(u.username LIKE @search OR u.display_name LIKE @search)");
            command.Parameters.AddWithValue("@search", $"%{query.Search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            where.Add("u.player_status = @status");
            command.Parameters.AddWithValue("@status", query.Status.Trim());
        }

        return where.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", where);
    }

    private static string BuildOrderBy(AdminPlayersQueryRequest query)
    {
        string direction = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        string column = query.SortBy switch
        {
            "username" => "u.username",
            "displayName" => "u.display_name",
            "status" => "u.player_status",
            "totalPoints" => "pp.total_score",
            "totalMatches" => "pp.total_matches_played",
            "softCurrency" => "pp.soft_currency",
            "lastLogin" => "u.last_login_at",
            "createdAt" => "u.created_at",
            _ => "u.created_at"
        };

        return $"ORDER BY {column} {direction}";
    }

    public async Task<AdminPlayerDetailDto?> GetPlayerDetailAsync(ulong userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  u.user_id,
                                  u.username,
                                  u.display_name,
                                  u.player_status,
                                  u.created_at,
                                  u.last_login_at,
                                  pp.soft_currency,
                                  pp.total_score,
                                  pp.total_matches_played,
                                  pp.total_matches_won,
                                  pp.total_trash_recycled,
                                  pp.total_revives_done,
                                  pp.total_times_fainted,
                                  pp.total_playtime_seconds
                              FROM users u
                              JOIN player_profiles pp ON pp.user_id = u.user_id
                              WHERE u.user_id = @userId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new AdminPlayerDetailDto
        {
            UserId = reader.GetUInt64("user_id"),
            Username = reader.GetString("username"),
            DisplayName = reader.GetString("display_name"),
            PlayerStatus = reader.GetString("player_status"),
            CreatedAt = reader.GetDateTime("created_at"),
            LastLoginAt = reader.IsDBNull(reader.GetOrdinal("last_login_at"))
                ? null
                : reader.GetDateTime("last_login_at"),

            SoftCurrency = reader.GetUInt32("soft_currency"),
            TotalScore = reader.GetUInt32("total_score"),
            TotalMatchesPlayed = reader.GetUInt32("total_matches_played"),
            TotalMatchesWon = reader.GetUInt32("total_matches_won"),
            TotalTrashRecycled = reader.GetUInt32("total_trash_recycled"),
            TotalRevivesDone = reader.GetUInt32("total_revives_done"),
            TotalTimesFainted = reader.GetUInt32("total_times_fainted"),
            TotalPlaytimeSeconds = reader.GetUInt32("total_playtime_seconds")
        };
    }

    public async Task<List<AdminPlayerInventoryItemDto>> GetPlayerInventoryAsync(ulong userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  pi.inventory_id,
                                  pi.shop_item_id,
                                  pi.quantity,
                                  pi.acquired_at,
                                  si.item_code,
                                  si.item_name,
                                  si.item_type
                              FROM player_inventory pi
                              JOIN shop_items si ON si.shop_item_id = pi.shop_item_id
                              WHERE pi.user_id = @userId
                              ORDER BY si.item_type, si.item_name;
                              """;

        command.Parameters.AddWithValue("@userId", userId);

        var items = new List<AdminPlayerInventoryItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminPlayerInventoryItemDto
            {
                InventoryId = reader.GetUInt64("inventory_id"),
                ShopItemId = reader.GetUInt64("shop_item_id"),
                Quantity = reader.GetUInt32("quantity"),
                AcquiredAt = reader.GetDateTime("acquired_at"),
                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemType = reader.GetString("item_type")
            });
        }

        return items;
    }

    public async Task<List<AdminPlayerRecentMatchDto>> GetPlayerRecentMatchesAsync(
        ulong userId,
        int limit = 10)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  m.match_id,
                                  m.match_code,
                                  m.started_at,
                                  m.ended_at,
                                  m.duration_seconds,
                                  mp.final_score,
                                  mp.trash_recycled_count,
                                  mp.revives_done,
                                  mp.times_fainted,
                                  mp.earned_currency
                              FROM match_players mp
                              JOIN matches m ON m.match_id = mp.match_id
                              WHERE mp.user_id = @userId
                              ORDER BY m.started_at DESC
                              LIMIT @limit;
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@limit", Math.Clamp(limit, 1, 50));

        var matches = new List<AdminPlayerRecentMatchDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            matches.Add(new AdminPlayerRecentMatchDto
            {
                MatchId = reader.GetUInt64("match_id"),
                MatchCode = reader.GetString("match_code"),
                StartedAt = reader.GetDateTime("started_at"),
                EndedAt = reader.GetDateTime("ended_at"),
                DurationSeconds = reader.GetUInt32("duration_seconds"),
                FinalScore = reader.GetUInt32("final_score"),
                TrashRecycledCount = reader.GetUInt32("trash_recycled_count"),
                RevivesDone = reader.GetUInt32("revives_done"),
                TimesFainted = reader.GetUInt32("times_fainted"),
                EarnedCurrency = reader.GetUInt32("earned_currency")
            });
        }

        return matches;
    }
}