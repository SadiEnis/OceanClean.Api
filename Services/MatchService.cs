using OceanClean.Api.DTOs.Matches;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class MatchService
{
    private readonly MatchRepository _matchRepository;

    public MatchService(MatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }
    
    public async Task<CompleteMatchResponse> CompleteMatchAsync(CompleteMatchRequest request)
    {
        var validationError = ValidateCompleteMatchRequest(request);
        // Request Null Check
        
        if (validationError != null)
        {
            return new CompleteMatchResponse
            {
                Success = false,
                Message = validationError
            };
        }

        var matchId = await _matchRepository.CompleteMatchAsync(request);

        return new CompleteMatchResponse
        {
            Success = true,
            Message = "Match completed successfully.",
            MatchId = matchId,
            MatchCode = request.MatchCode
        };
    }
    
    private static string? ValidateCompleteMatchRequest(CompleteMatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MatchCode))
            return "Match code is required.";

        if (request.StartedAt == default)
            return "StartedAt is required.";

        if (request.EndedAt == default)
            return "EndedAt is required.";

        if (request.EndedAt < request.StartedAt)
            return "EndedAt cannot be earlier than StartedAt.";

        if (request.DurationSeconds == 0)
            return "DurationSeconds must be greater than zero.";

        if (request.Players == null || request.Players.Count == 0)
            return "At least one player result is required.";

        var duplicateUserId = request.Players
            .GroupBy(player => player.UserId)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateUserId != null)
            return $"Duplicate player result found for user_id={duplicateUserId.Key}.";

        foreach (var player in request.Players)
        {
            if (player.UserId == 0)
                return "Player userId must be greater than zero.";
        }
        
        if (request.UsedItems != null)
        {
            foreach (var usedItem in request.UsedItems)
            {
                if (usedItem.UserId == 0)
                    return "Used item userId must be greater than zero.";

                if (string.IsNullOrWhiteSpace(usedItem.ItemCode))
                    return "Used item code is required.";

                if (usedItem.Quantity == 0)
                    return "Used item quantity must be greater than zero.";
            }
        }
        
        if (request.ActionLogs != null)
        {
            foreach (var actionLog in request.ActionLogs)
            {
                if (actionLog.UserId == 0)
                    return "Action log userId must be greater than zero.";

                if (string.IsNullOrWhiteSpace(actionLog.ActionType))
                    return "Action log actionType is required.";

                if (actionLog.CreatedAt == default)
                    return "Action log createdAt is required.";
            }
        }

        return null;
    }
}