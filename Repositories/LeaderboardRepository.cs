using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Leaderboard;

namespace OceanClean.Api.Repositories;

public class LeaderboardRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public LeaderboardRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  u.user_id,
                                  u.username,
                                  u.display_name,
                                  pp.total_score,
                                  pp.total_matches_played,
                                  pp.total_trash_recycled
                              FROM player_profiles pp
                              INNER JOIN users u ON u.user_id = pp.user_id
                              WHERE u.player_status = 'active'
                              ORDER BY
                                  pp.total_score DESC,
                                  pp.total_trash_recycled DESC,
                                  u.user_id ASC
                              LIMIT @limit;
                              """;

        command.Parameters.AddWithValue("@limit", limit);

        var players = new List<LeaderboardEntryDto>();

        await using var reader = await command.ExecuteReaderAsync();

        int rank = 1;

        while (await reader.ReadAsync())
        {
            players.Add(new LeaderboardEntryDto
            {
                Rank = rank,
                UserId = reader.GetUInt64("user_id"),
                Username = reader.GetString("username"),
                DisplayName = reader.GetString("display_name"),
                TotalScore = reader.GetInt32("total_score"),
                TotalMatchesPlayed = reader.GetInt32("total_matches_played"),
                TotalTrashRecycled = reader.GetInt32("total_trash_recycled")
            });

            rank++;
        }

        return players;
    }
}