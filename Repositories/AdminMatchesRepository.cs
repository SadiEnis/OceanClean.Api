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
}