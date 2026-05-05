using OceanClean.Api.Data;
using OceanClean.Api.Models.Player;

namespace OceanClean.Api.Repositories;

public class PlayerProfileRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public PlayerProfileRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PlayerProfileRecord> GetProfileByUserIdAsync(ulong userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT 
                                u.user_id,
                                u.username,
                                u.display_name,

                                pp.soft_currency,
                                pp.total_score,
                                pp.total_matches_played,
                                pp.total_matches_won,
                                pp.total_trash_recycled,
                                pp.total_revives_done,
                                pp.total_times_fainted,
                                pp.total_playtime_seconds
                              FROM player_profiles pp
                                INNER JOIN users u On u.user_id = pp.user_id
                                    WHERE u.user_id = @userId
                                        LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new PlayerProfileRecord {  
            UserId = reader.GetUInt64("user_id"),
            Username = reader.GetString("username"),
            DisplayName = reader.GetString("display_name"),

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
}