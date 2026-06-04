using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.AuditLogs;

namespace OceanClean.Api.Repositories.Admin;

public class AdminAuditLogsRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminAuditLogsRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<AdminAuditLogListItemDto> Items, long TotalCount)> GetAuditLogsAsync(
        AdminAuditLogsQueryRequest query,
        ulong currentAdminUserId,
        string currentAdminRole)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var whereClauses = new List<string>();
        var parameters = new List<MySqlParameter>();

        bool canViewAllLogs =
            currentAdminRole.Equals("super_admin", StringComparison.OrdinalIgnoreCase) ||
            currentAdminRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

        if (!canViewAllLogs)
        {
            whereClauses.Add("aal.admin_user_id = @currentAdminUserId");
            parameters.Add(new MySqlParameter("@currentAdminUserId", currentAdminUserId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            whereClauses.Add("""
                             (
                                 au.username LIKE @search OR
                                 aal.action_type LIKE @search OR
                                 aal.target_type LIKE @search OR
                                 aal.old_value LIKE @search OR
                                 aal.new_value LIKE @search OR
                                 aal.ip_address LIKE @search
                             )
                             """);

            parameters.Add(new MySqlParameter("@search", $"%{query.Search.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.ActionType))
        {
            whereClauses.Add("aal.action_type = @actionType");
            parameters.Add(new MySqlParameter("@actionType", query.ActionType.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.TargetType))
        {
            whereClauses.Add("aal.target_type = @targetType");
            parameters.Add(new MySqlParameter("@targetType", query.TargetType.Trim()));
        }

        if (query.From.HasValue)
        {
            whereClauses.Add("aal.created_at >= @from");
            parameters.Add(new MySqlParameter("@from", query.From.Value));
        }

        if (query.To.HasValue)
        {
            whereClauses.Add("aal.created_at <= @to");
            parameters.Add(new MySqlParameter("@to", query.To.Value));
        }

        string whereSql = whereClauses.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", whereClauses);

        string orderBy = ResolveOrderBy(query.SortBy);
        string sortDirection = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, 100);
        int offset = (page - 1) * pageSize;

        long totalCount = await GetTotalCountAsync(connection, whereSql, parameters);

        var items = await GetItemsAsync(
            connection,
            whereSql,
            parameters,
            orderBy,
            sortDirection,
            pageSize,
            offset
        );

        return (items, totalCount);
    }

    private static async Task<long> GetTotalCountAsync(
        MySqlConnection connection,
        string whereSql,
        List<MySqlParameter> parameters)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = $"""
                               SELECT COUNT(*)
                               FROM admin_audit_logs aal
                               INNER JOIN admin_users au ON au.admin_user_id = aal.admin_user_id
                               {whereSql};
                               """;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(new MySqlParameter(parameter.ParameterName, parameter.Value));
        }

        object? result = await command.ExecuteScalarAsync();

        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt64(result);
    }

    private static async Task<List<AdminAuditLogListItemDto>> GetItemsAsync(
        MySqlConnection connection,
        string whereSql,
        List<MySqlParameter> parameters,
        string orderBy,
        string sortDirection,
        int pageSize,
        int offset)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = $"""
                               SELECT
                                   aal.audit_log_id,
                                   aal.admin_user_id,
                                   au.username AS admin_username,
                                   au.admin_role,
                                   aal.action_type,
                                   aal.target_type,
                                   aal.target_id,
                                   aal.old_value,
                                   aal.new_value,
                                   aal.ip_address,
                                   aal.user_agent,
                                   aal.created_at
                               FROM admin_audit_logs aal
                               INNER JOIN admin_users au ON au.admin_user_id = aal.admin_user_id
                               {whereSql}
                               ORDER BY {orderBy} {sortDirection}
                               LIMIT @pageSize OFFSET @offset;
                               """;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(new MySqlParameter(parameter.ParameterName, parameter.Value));
        }

        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@offset", offset);

        var items = new List<AdminAuditLogListItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminAuditLogListItemDto
            {
                AuditLogId = reader.GetUInt64("audit_log_id"),
                AdminUserId = reader.GetUInt64("admin_user_id"),
                AdminUsername = reader.GetString("admin_username"),
                AdminRole = reader.GetString("admin_role"),
                ActionType = reader.GetString("action_type"),
                TargetType = reader.GetString("target_type"),
                TargetId = reader.IsDBNull(reader.GetOrdinal("target_id"))
                    ? null
                    : reader.GetUInt64("target_id"),
                OldValue = reader.IsDBNull(reader.GetOrdinal("old_value"))
                    ? null
                    : reader.GetString("old_value"),
                NewValue = reader.IsDBNull(reader.GetOrdinal("new_value"))
                    ? null
                    : reader.GetString("new_value"),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                    ? null
                    : reader.GetString("ip_address"),
                UserAgent = reader.IsDBNull(reader.GetOrdinal("user_agent"))
                    ? null
                    : reader.GetString("user_agent"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return items;
    }

    private static string ResolveOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "adminUsername" => "au.username",
            "adminRole" => "au.admin_role",
            "actionType" => "aal.action_type",
            "targetType" => "aal.target_type",
            "targetId" => "aal.target_id",
            "createdAt" => "aal.created_at",
            _ => "aal.created_at"
        };
    }
}