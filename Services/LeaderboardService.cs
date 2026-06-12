using OceanClean.Api.DTOs.Leaderboard;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class LeaderboardService
{
    private readonly LeaderboardRepository _leaderboardRepository;

    public LeaderboardService(LeaderboardRepository leaderboardRepository)
    {
        _leaderboardRepository = leaderboardRepository;
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(int limit)
    {
        if (limit <= 0)
            limit = 10;

        if (limit > 50)
            limit = 50;

        var players = await _leaderboardRepository.GetLeaderboardAsync(limit);

        return new LeaderboardResponse
        {
            Success = true,
            Message = "Leaderboard retrieved successfully.",
            Players = players
        };
    }
}