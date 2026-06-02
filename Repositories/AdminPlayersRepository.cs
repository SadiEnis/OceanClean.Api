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
}