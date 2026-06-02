using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.Matches;
using OceanClean.Api.Models.Admin;

namespace OceanClean.Api.Repositories;

public class AdminMatchesRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminMatchesRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> GetMatchesCountAsync(AdminMatchesQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClauses = BuildWhereClauses(query, command);

        command.CommandText = $"""
                               SELECT COUNT(*)
                               FROM matches m
                               {whereClauses};
                               """;

        object? result = await command.ExecuteScalarAsync();

        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt64(result);
    }

    public async Task<List<AdminMatchListItemRecord>> GetMatchesAsync(AdminMatchesQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClauses = BuildWhereClauses(query, command);
        string orderBy = BuildOrderBy(query);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, 100);
        int offset = (page - 1) * pageSize;

        command.CommandText = $"""
                               SELECT
                                   m.match_id,
                                   m.match_code,
                                   m.started_at,
                                   m.ended_at,
                                   m.duration_seconds,
                                   m.total_trash_spawned,
                                   m.total_trash_recycled,
                                   m.created_at,
                                   COUNT(mp.user_id) AS player_count,
                                   COALESCE(SUM(mp.final_score), 0) AS total_score,
                                   COALESCE(SUM(mp.earned_currency), 0) AS total_currency_earned
                               FROM matches m
                               LEFT JOIN match_players mp ON mp.match_id = m.match_id
                               {whereClauses}
                               GROUP BY
                                   m.match_id,
                                   m.match_code,
                                   m.started_at,
                                   m.ended_at,
                                   m.duration_seconds,
                                   m.total_trash_spawned,
                                   m.total_trash_recycled,
                                   m.created_at
                               {orderBy}
                               LIMIT @pageSize OFFSET @offset;
                               """;

        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@offset", offset);

        var matches = new List<AdminMatchListItemRecord>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            matches.Add(new AdminMatchListItemRecord
            {
                MatchId = reader.GetUInt64("match_id"),
                MatchCode = reader.GetString("match_code"),
                StartedAt = reader.GetDateTime("started_at"),
                EndedAt = reader.GetDateTime("ended_at"),
                DurationSeconds = reader.GetUInt32("duration_seconds"),
                TotalTrashSpawned = reader.GetUInt32("total_trash_spawned"),
                TotalTrashRecycled = reader.GetUInt32("total_trash_recycled"),
                CreatedAt = reader.GetDateTime("created_at"),
                PlayerCount = Convert.ToUInt32(reader.GetInt64("player_count")),
                TotalScore = Convert.ToUInt32(reader.GetDecimal("total_score")),
                TotalCurrencyEarned = Convert.ToUInt32(reader.GetDecimal("total_currency_earned"))
            });
        }

        return matches;
    }

    private static string BuildWhereClauses(
        AdminMatchesQueryRequest query,
        MySqlCommand command)
    {
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            where.Add("m.match_code LIKE @search");
            command.Parameters.AddWithValue("@search", $"%{query.Search.Trim()}%");
        }

        if (query.StartedFrom.HasValue)
        {
            where.Add("m.started_at >= @startedFrom");
            command.Parameters.AddWithValue("@startedFrom", query.StartedFrom.Value);
        }

        if (query.StartedTo.HasValue)
        {
            where.Add("m.started_at <= @startedTo");
            command.Parameters.AddWithValue("@startedTo", query.StartedTo.Value);
        }

        return where.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", where);
    }

    private static string BuildOrderBy(AdminMatchesQueryRequest query)
    {
        string direction = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        string column = query.SortBy switch
        {
            "matchCode" => "m.match_code",
            "startedAt" => "m.started_at",
            "endedAt" => "m.ended_at",
            "durationSeconds" => "m.duration_seconds",
            "totalTrashSpawned" => "m.total_trash_spawned",
            "totalTrashRecycled" => "m.total_trash_recycled",
            "playerCount" => "player_count",
            "totalScore" => "total_score",
            "totalCurrencyEarned" => "total_currency_earned",
            "createdAt" => "m.created_at",
            _ => "m.started_at"
        };

        return $"ORDER BY {column} {direction}";
    }

    public async Task<AdminMatchDetailDto?> GetMatchDetailAsync(ulong matchId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  match_id,
                                  lobby_id,
                                  match_code,
                                  started_at,
                                  ended_at,
                                  duration_seconds,
                                  total_trash_spawned,
                                  total_trash_recycled,
                                  created_at
                              FROM matches
                              WHERE match_id = @matchId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new AdminMatchDetailDto
        {
            MatchId = reader.GetUInt64("match_id"),
            LobbyId = reader.IsDBNull(reader.GetOrdinal("lobby_id"))
                ? null
                : reader.GetUInt64("lobby_id"),
            MatchCode = reader.GetString("match_code"),
            StartedAt = reader.GetDateTime("started_at"),
            EndedAt = reader.GetDateTime("ended_at"),
            DurationSeconds = reader.GetUInt32("duration_seconds"),
            TotalTrashSpawned = reader.GetUInt32("total_trash_spawned"),
            TotalTrashRecycled = reader.GetUInt32("total_trash_recycled"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }

    public async Task<List<AdminMatchPlayerDto>> GetMatchPlayersAsync(ulong matchId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  mp.user_id,
                                  u.username,
                                  u.display_name,
                                  mp.final_score,
                                  mp.trash_recycled_count,
                                  mp.revives_done,
                                  mp.times_fainted,
                                  mp.earned_currency
                              FROM match_players mp
                              JOIN users u ON u.user_id = mp.user_id
                              WHERE mp.match_id = @matchId
                              ORDER BY mp.final_score DESC;
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);

        var players = new List<AdminMatchPlayerDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            players.Add(new AdminMatchPlayerDto
            {
                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),
                FinalScore = reader.GetUInt32("final_score"),
                TrashRecycledCount = reader.GetUInt32("trash_recycled_count"),
                RevivesDone = reader.GetUInt32("revives_done"),
                TimesFainted = reader.GetUInt32("times_fainted"),
                EarnedCurrency = reader.GetUInt32("earned_currency")
            });
        }

        return players;
    }

    public async Task<List<AdminMatchUsedItemDto>> GetMatchUsedItemsAsync(ulong matchId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  pal.log_id,
                                  pal.user_id,
                                  u.username,
                                  u.display_name,
                                  pal.shop_item_id,
                                  si.item_code,
                                  si.item_name,
                                  si.item_type,
                                  pal.value,
                                  pal.created_at
                              FROM player_action_logs pal
                              JOIN users u ON u.user_id = pal.user_id
                              LEFT JOIN shop_items si ON si.shop_item_id = pal.shop_item_id
                              WHERE pal.match_id = @matchId
                                AND pal.action_type = 'use_item'
                              ORDER BY pal.created_at ASC;
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);

        var usedItems = new List<AdminMatchUsedItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usedItems.Add(new AdminMatchUsedItemDto
            {
                LogId = reader.GetUInt64("log_id"),
                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),

                ShopItemId = reader.IsDBNull(reader.GetOrdinal("shop_item_id"))
                    ? null
                    : reader.GetUInt64("shop_item_id"),
                ItemCode = reader.IsDBNull(reader.GetOrdinal("item_code"))
                    ? null
                    : reader.GetString("item_code"),
                ItemName = reader.IsDBNull(reader.GetOrdinal("item_name"))
                    ? null
                    : reader.GetString("item_name"),
                ItemType = reader.IsDBNull(reader.GetOrdinal("item_type"))
                    ? null
                    : reader.GetString("item_type"),

                Quantity = reader.IsDBNull(reader.GetOrdinal("value"))
                    ? null
                    : reader.GetInt32("value"),

                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return usedItems;
    }

    public async Task<List<AdminMatchRescueEventDto>> GetMatchRescueEventsAsync(ulong matchId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  pal.log_id,
                                  pal.user_id,
                                  u.username,
                                  u.display_name,
                                  pal.value,
                                  pal.pos_x,
                                  pal.pos_y,
                                  pal.created_at
                              FROM player_action_logs pal
                              JOIN users u ON u.user_id = pal.user_id
                              WHERE pal.match_id = @matchId
                                AND pal.action_type = 'rescue_completed'
                              ORDER BY pal.created_at ASC;
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);

        var events = new List<AdminMatchRescueEventDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            events.Add(new AdminMatchRescueEventDto
            {
                LogId = reader.GetUInt64("log_id"),
                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),
                ScoreReward = reader.IsDBNull(reader.GetOrdinal("value"))
                    ? null
                    : reader.GetInt32("value"),
                PosX = reader.IsDBNull(reader.GetOrdinal("pos_x"))
                    ? null
                    : reader.GetFloat("pos_x"),
                PosY = reader.IsDBNull(reader.GetOrdinal("pos_y"))
                    ? null
                    : reader.GetFloat("pos_y"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return events;
    }

    public async Task<List<AdminMatchFaintReviveEventDto>> GetMatchFaintReviveEventsAsync(ulong matchId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  pal.log_id,
                                  pal.action_type,
                                  pal.user_id,
                                  u.username,
                                  u.display_name,
                                  pal.target_user_id,
                                  target.username AS target_username,
                                  target.display_name AS target_display_name,
                                  pal.value,
                                  pal.pos_x,
                                  pal.pos_y,
                                  pal.created_at
                              FROM player_action_logs pal
                              JOIN users u ON u.user_id = pal.user_id
                              LEFT JOIN users target ON target.user_id = pal.target_user_id
                              WHERE pal.match_id = @matchId
                                AND pal.action_type IN ('player_fainted', 'revive_player')
                              ORDER BY pal.created_at ASC;
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);

        var events = new List<AdminMatchFaintReviveEventDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            events.Add(new AdminMatchFaintReviveEventDto
            {
                LogId = reader.GetUInt64("log_id"),
                ActionType = reader.GetString("action_type"),

                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),

                TargetUserId = reader.IsDBNull(reader.GetOrdinal("target_user_id"))
                    ? null
                    : reader.GetUInt64("target_user_id"),
                TargetUsername = reader.IsDBNull(reader.GetOrdinal("target_username"))
                    ? null
                    : reader.GetString("target_username"),
                TargetDisplayName = reader.IsDBNull(reader.GetOrdinal("target_display_name"))
                    ? null
                    : reader.GetString("target_display_name"),

                Value = reader.IsDBNull(reader.GetOrdinal("value"))
                    ? null
                    : reader.GetInt32("value"),
                PosX = reader.IsDBNull(reader.GetOrdinal("pos_x"))
                    ? null
                    : reader.GetFloat("pos_x"),
                PosY = reader.IsDBNull(reader.GetOrdinal("pos_y"))
                    ? null
                    : reader.GetFloat("pos_y"),

                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return events;
    }
}