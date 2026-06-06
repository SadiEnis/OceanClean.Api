using OceanClean.Api.Data;
using OceanClean.Api.Models.Users;

namespace OceanClean.Api.Repositories;

public class UserRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public UserRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> UsernameExistAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT COUNT(*) FROM users
                              WHERE username = @username;
                              """;

        command.Parameters.AddWithValue("@username", username);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        return result > 0;
    }

    public async Task<ulong> CreateUserWithProfileAsync(
        string username,
        string passwordHash,
        string displayName)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var insertUserCommand = connection.CreateCommand();
            insertUserCommand.Transaction = transaction;
            insertUserCommand.CommandText = """
                                            INSERT INTO users
                                            (
                                                username,
                                                password_hash,
                                                display_name
                                            )
                                            VALUES
                                            (
                                                @username,
                                                @passwordHash,
                                                @displayName
                                            );

                                            SELECT LAST_INSERT_ID();
                                            """;

            insertUserCommand.Parameters.AddWithValue("@username", username);
            insertUserCommand.Parameters.AddWithValue("@passwordHash", passwordHash);
            insertUserCommand.Parameters.AddWithValue("@displayName", displayName);

            var userId = Convert.ToUInt64(await insertUserCommand.ExecuteScalarAsync());

            await using var insertProfileCommand = connection.CreateCommand();
            insertProfileCommand.Transaction = transaction;
            insertProfileCommand.CommandText = """
                                               INSERT INTO player_profiles
                                               (
                                                   user_id
                                               )
                                               VALUES
                                               (
                                                   @userId
                                               );
                                               """;

            insertProfileCommand.Parameters.AddWithValue("@userId", userId);

            await insertProfileCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            return userId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<UserRecord?> GetUserByUsernameAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT
                                  user_id,
                                  username,
                                  password_hash,
                                  display_name,
                                  player_status
                              FROM users
                              WHERE username = @username
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@username", username);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new UserRecord
        {
            UserId = reader.GetUInt64("user_id"),
            Username = reader.GetString("username"),
            PasswordHash = reader.GetString("password_hash"),
            DisplayName = reader.GetString("display_name"),
            Status = reader.GetString("player_status")
        };
    }

    public async Task UpdateLastLoginAsync(ulong userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              UPDATE users
                              SET last_login_at = UTC_TIMESTAMP()
                              WHERE user_id = @userId;
                              """;

        command.Parameters.AddWithValue("@userId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertPlayerLoginLogAsync(
        ulong userId,
        string? ipAddress,
        string? userAgent)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO player_login_logs
                              (
                                  user_id,
                                  ip_address,
                                  user_agent,
                                  created_at
                              )
                              VALUES
                              (
                                  @userId,
                                  @ipAddress,
                                  @userAgent,
                                  UTC_TIMESTAMP()
                              );
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@userAgent", userAgent ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
}