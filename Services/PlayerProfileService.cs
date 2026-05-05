using OceanClean.Api.DTOs.Player;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class PlayerProfileService
{
    private readonly PlayerProfileRepository _playerProfileRepository;

    public PlayerProfileService(PlayerProfileRepository playerProfileRepository)
    {
        _playerProfileRepository = playerProfileRepository;
    }

    public async Task<PlayerProfileResponse?> GetProfileAsync(ulong userId)
    {
        var profile = await _playerProfileRepository.GetProfileByUserIdAsync(userId);

        if (profile == null)
            return null;

        return new PlayerProfileResponse
        {
            Success = true,
            Message = "Player profile retrieved successfully.",

            UserId = profile.UserId,
            Username = profile.Username,
            DisplayName = profile.DisplayName,

            SoftCurrency = profile.SoftCurrency,
            TotalScore = profile.TotalScore,

            TotalMatchesPlayed = profile.TotalMatchesPlayed,
            TotalMatchesWon = profile.TotalMatchesWon,

            TotalTrashRecycled = profile.TotalTrashRecycled,

            TotalRevivesDone = profile.TotalRevivesDone,
            TotalTimesFainted = profile.TotalTimesFainted,
            TotalPlaytimeSeconds = profile.TotalPlaytimeSeconds
        };
    }
}