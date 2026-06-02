using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.Events;

namespace OceanClean.Api.Repositories.Admin;

public class AdminEventsRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminEventsRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> GetEventsCountAsync(AdminEventsQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClause = BuildWhereClause(query, command);

        command.CommandText = $"""
                               SELECT COUNT(*)
                               FROM player_action_logs pal
                               JOIN users u ON u.user_id = pal.user_id
                               JOIN matches m ON m.match_id = pal.match_id
                               {whereClause};
                               """;

        object? result = await command.ExecuteScalarAsync();

        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt64(result);
    }

    public async Task<List<AdminEventLogItemDto>> GetEventsAsync(AdminEventsQueryRequest query)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClause = BuildWhereClause(query, command);
        string orderBy = BuildOrderBy(query);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, 100);
        int offset = (page - 1) * pageSize;

        command.CommandText = $"""
                               SELECT
                                   pal.log_id,
                                   pal.created_at,
                                   pal.action_type,

                                   pal.user_id,
                                   u.username,
                                   u.display_name,

                                   pal.match_id,
                                   m.match_code,

                                   pal.target_user_id,
                                   target.username AS target_username,
                                   target.display_name AS target_display_name,

                                   pal.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type,

                                   pal.trash_type_id,
                                   tt.trash_type_code,
                                   tt.trash_type_name,

                                   pal.value,
                                   pal.pos_x,
                                   pal.pos_y
                               FROM player_action_logs pal
                               JOIN users u ON u.user_id = pal.user_id
                               JOIN matches m ON m.match_id = pal.match_id
                               LEFT JOIN users target ON target.user_id = pal.target_user_id
                               LEFT JOIN shop_items si ON si.shop_item_id = pal.shop_item_id
                               LEFT JOIN trash_types tt ON tt.trash_type_id = pal.trash_type_id
                               {whereClause}
                               {orderBy}
                               LIMIT @pageSize OFFSET @offset;
                               """;

        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@offset", offset);

        var events = new List<AdminEventLogItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            events.Add(new AdminEventLogItemDto
            {
                LogId = reader.GetUInt64("log_id"),
                CreatedAt = reader.GetDateTime("created_at"),
                ActionType = reader.GetString("action_type"),

                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),

                MatchId = reader.GetUInt64("match_id"),
                MatchCode = reader.GetString("match_code"),

                TargetUserId = reader.IsDBNull(reader.GetOrdinal("target_user_id"))
                    ? null
                    : reader.GetUInt64("target_user_id"),
                TargetUsername = reader.IsDBNull(reader.GetOrdinal("target_username"))
                    ? null
                    : reader.GetString("target_username"),
                TargetDisplayName = reader.IsDBNull(reader.GetOrdinal("target_display_name"))
                    ? null
                    : reader.GetString("target_display_name"),

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

                TrashTypeId = reader.IsDBNull(reader.GetOrdinal("trash_type_id"))
                    ? null
                    : reader.GetUInt64("trash_type_id"),
                TrashTypeCode = reader.IsDBNull(reader.GetOrdinal("trash_type_code"))
                    ? null
                    : reader.GetString("trash_type_code"),
                TrashTypeName = reader.IsDBNull(reader.GetOrdinal("trash_type_name"))
                    ? null
                    : reader.GetString("trash_type_name"),

                Value = reader.IsDBNull(reader.GetOrdinal("value"))
                    ? null
                    : reader.GetInt32("value"),
                PosX = reader.IsDBNull(reader.GetOrdinal("pos_x"))
                    ? null
                    : reader.GetFloat("pos_x"),
                PosY = reader.IsDBNull(reader.GetOrdinal("pos_y"))
                    ? null
                    : reader.GetFloat("pos_y")
            });
        }

        return events;
    }

    private static string BuildWhereClause(
        AdminEventsQueryRequest query,
        MySqlCommand command)
    {
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Username))
        {
            where.Add("(u.username LIKE @username OR u.display_name LIKE @username)");
            command.Parameters.AddWithValue("@username", $"%{query.Username.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(query.MatchCode))
        {
            where.Add("m.match_code LIKE @matchCode");
            command.Parameters.AddWithValue("@matchCode", $"%{query.MatchCode.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(query.ActionType))
        {
            where.Add("pal.action_type = @actionType");
            command.Parameters.AddWithValue("@actionType", query.ActionType.Trim());
        }

        if (query.From.HasValue)
        {
            where.Add("pal.created_at >= @from");
            command.Parameters.AddWithValue("@from", query.From.Value);
        }

        if (query.To.HasValue)
        {
            where.Add("pal.created_at <= @to");
            command.Parameters.AddWithValue("@to", query.To.Value);
        }

        return where.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", where);
    }

    private static string BuildOrderBy(AdminEventsQueryRequest query)
    {
        string direction = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        string column = query.SortBy switch
        {
            "createdAt" => "pal.created_at",
            "actionType" => "pal.action_type",
            "username" => "u.username",
            "displayName" => "u.display_name",
            "matchCode" => "m.match_code",
            "value" => "pal.value",
            _ => "pal.created_at"
        };

        return $"ORDER BY {column} {direction}";
    }
}