using OceanClean.Api.Data;
using OceanClean.Api.Models.Admin;

namespace OceanClean.Api.Repositories;

public class AdminAuthRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminAuthRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminUserRecord> GetAdminByUsernameAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT
                                  admin_user_id,
                                  username,
                                  password_hash,
                                  admin_role,
                                  admin_status
                              FROM admin_users
                              WHERE username = @username
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@username", username);
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync()) return  null;

        return new AdminUserRecord
        {
            AdminUserId = reader.GetUInt64("admin_user_id"),
            Username = reader.GetString("username"),
            PasswordHash = reader.GetString("password_hash"),
            AdminRole = reader.GetString("admin_role"),
            AdminStatus = reader.GetString("admin_status")
        };
    }
    
    public async Task SaveRefreshTokenAsync(
        ulong adminUserId,
        string tokenHash,
        DateTime expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO admin_refresh_tokens
                              (
                                  admin_user_id,
                                  token_hash,
                                  expires_at,
                                  revoked_at,
                                  created_at,
                                  created_by_ip,
                                  revoked_by_ip,
                                  user_agent
                              )
                              VALUES
                              (
                                  @adminUserId,
                                  @tokenHash,
                                  @expiresAt,
                                  NULL,
                                  UTC_TIMESTAMP(),
                                  @createdByIp,
                                  NULL,
                                  @userAgent
                              );
                              """;

        command.Parameters.AddWithValue("@adminUserId", adminUserId);
        command.Parameters.AddWithValue("@tokenHash", tokenHash);
        command.Parameters.AddWithValue("@expiresAt", expiresAt);
        command.Parameters.AddWithValue("@createdByIp", ipAddress ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@userAgent", userAgent ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateLastLoginAsync(ulong adminUserId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE admin_users
                              SET last_login = UTC_TIMESTAMP(),
                                  updated_at = UTC_TIMESTAMP()
                              WHERE admin_user_id = @adminUserId;
                              """;

        command.Parameters.AddWithValue("@adminUserId", adminUserId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertAdminAuditLogAsync(
        ulong adminUserId,
        string actionType,
        string targetType,
        ulong? targetId,
        string? oldValue,
        string? newValue,
        string? ipAddress,
        string? userAgent)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO admin_audit_logs
                              (
                                  admin_user_id,
                                  action_type,
                                  target_type,
                                  target_id,
                                  old_value,
                                  new_value,
                                  ip_address,
                                  user_agent,
                                  created_at
                              )
                              VALUES
                              (
                                  @adminUserId,
                                  @actionType,
                                  @targetType,
                                  @targetId,
                                  @oldValue,
                                  @newValue,
                                  @ipAddress,
                                  @userAgent,
                                  UTC_TIMESTAMP()
                              );
                              """;

        command.Parameters.AddWithValue("@adminUserId", adminUserId);
        command.Parameters.AddWithValue("@actionType", actionType);
        command.Parameters.AddWithValue("@targetType", targetType);
        command.Parameters.AddWithValue("@targetId", targetId.HasValue ? targetId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@oldValue", oldValue ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@newValue", newValue ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@userAgent", userAgent ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<AdminUserRecord?> GetAdminByIdAsync(ulong adminUserId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  admin_user_id,
                                  username,
                                  password_hash,
                                  admin_role,
                                  admin_status
                              FROM admin_users
                              WHERE admin_user_id = @adminUserId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@adminUserId", adminUserId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new AdminUserRecord
        {
            AdminUserId = reader.GetUInt64("admin_user_id"),
            Username = reader.GetString("username"),
            PasswordHash = reader.GetString("password_hash"),
            AdminRole = reader.GetString("admin_role"),
            AdminStatus = reader.GetString("admin_status")
        };
    }
    
    public async Task<AdminRefreshTokenRecord?> GetRefreshTokenByHashAsync(string tokenHash)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  refresh_token_id,
                                  admin_user_id,
                                  token_hash,
                                  expires_at,
                                  revoked_at
                              FROM admin_refresh_tokens
                              WHERE token_hash = @tokenHash
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@tokenHash", tokenHash);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new AdminRefreshTokenRecord
        {
            RefreshTokenId = reader.GetUInt64("refresh_token_id"),
            AdminUserId = reader.GetUInt64("admin_user_id"),
            TokenHash = reader.GetString("token_hash"),
            ExpiresAt = reader.GetDateTime("expires_at"),
            RevokedAt = reader.IsDBNull(reader.GetOrdinal("revoked_at"))
                ? null
                : reader.GetDateTime("revoked_at")
        };
    }
    
    public async Task RevokeRefreshTokenAsync(
        ulong refreshTokenId,
        string? revokedByIp)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE admin_refresh_tokens
                              SET revoked_at = UTC_TIMESTAMP(),
                                  revoked_by_ip = @revokedByIp
                              WHERE refresh_token_id = @refreshTokenId
                                AND revoked_at IS NULL;
                              """;

        command.Parameters.AddWithValue("@refreshTokenId", refreshTokenId);
        command.Parameters.AddWithValue("@revokedByIp", revokedByIp ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
}