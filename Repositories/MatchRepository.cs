using System.Data.Common;
using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Matches;

namespace OceanClean.Api.Repositories;

public class MatchRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public MatchRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ulong> CompleteMatchAsync(CompleteMatchRequest request)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var matchId = await InsertMatchAsync(connection, transaction, request);

            foreach (var player in request.Players)
            {
                var balanceBefore = await GetSoftCurrencyAsync(connection, transaction, player.UserId);
                var balanceAfter = balanceBefore + player.EarnedCurrency;

                await InsertMatchPlayerAsync(connection, transaction, matchId, player);
                await UpdatePlayerProfileAsync(connection, transaction, player, request.DurationSeconds);
                await InsertCurrencyTransactionAsync(
                    connection,
                    transaction,
                    matchId,
                    player,
                    balanceBefore,
                    balanceAfter
                );
            }
            if (request.UsedItems is { Count: > 0 })
            {
                foreach (var usedItem in request.UsedItems)
                {
                    await ConsumeUsedItemAsync(connection, transaction, usedItem);
                }
            }

            await transaction.CommitAsync();

            return matchId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<ulong> InsertMatchAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        CompleteMatchRequest request)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              INSERT INTO matches (
                                  match_code,
                                  started_at,
                                  ended_at,
                                  duration_seconds,
                                  total_trash_spawned,
                                  total_trash_recycled
                              )
                              VALUES (
                                  @matchCode,
                                  @startedAt,
                                  @endedAt,
                                  @durationSeconds,
                                  @totalTrashSpawned,
                                  @totalTrashRecycled
                              );

                              SELECT LAST_INSERT_ID();
                              """;

        command.Parameters.AddWithValue("@matchCode", request.MatchCode);
        command.Parameters.AddWithValue("@startedAt", request.StartedAt);
        command.Parameters.AddWithValue("@endedAt", request.EndedAt);
        command.Parameters.AddWithValue("@durationSeconds", request.DurationSeconds);
        command.Parameters.AddWithValue("@totalTrashSpawned", request.TotalTrashSpawned);
        command.Parameters.AddWithValue("@totalTrashRecycled", request.TotalTrashRecycled);

        var result = await command.ExecuteScalarAsync();

        return Convert.ToUInt64(result);
    }

    private static async Task InsertMatchPlayerAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong matchId,
        MatchPlayerResultRequest player)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              INSERT INTO match_players (
                                  match_id,
                                  user_id,
                                  final_score,
                                  trash_recycled_count,
                                  revives_done,
                                  times_fainted,
                                  earned_currency
                              )
                              VALUES (
                                  @matchId,
                                  @userId,
                                  @finalScore,
                                  @trashRecycledCount,
                                  @revivesDone,
                                  @timesFainted,
                                  @earnedCurrency
                              );
                              """;

        command.Parameters.AddWithValue("@matchId", matchId);
        command.Parameters.AddWithValue("@userId", player.UserId);
        command.Parameters.AddWithValue("@finalScore", player.FinalScore);
        command.Parameters.AddWithValue("@trashRecycledCount", player.TrashRecycledCount);
        command.Parameters.AddWithValue("@revivesDone", player.RevivesDone);
        command.Parameters.AddWithValue("@timesFainted", player.TimesFainted);
        command.Parameters.AddWithValue("@earnedCurrency", player.EarnedCurrency);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task UpdatePlayerProfileAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        MatchPlayerResultRequest player,
        uint durationSeconds)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              UPDATE player_profiles
                              SET
                                  soft_currency = soft_currency + @earnedCurrency,
                                  total_score = total_score + @finalScore,
                                  total_matches_played = total_matches_played + 1,
                                  total_trash_recycled = total_trash_recycled + @trashRecycledCount,
                                  total_revives_done = total_revives_done + @revivesDone,
                                  total_times_fainted = total_times_fainted + @timesFainted,
                                  total_playtime_seconds = total_playtime_seconds + @durationSeconds
                              WHERE user_id = @userId;
                              """;

        command.Parameters.AddWithValue("@earnedCurrency", player.EarnedCurrency);
        command.Parameters.AddWithValue("@finalScore", player.FinalScore);
        command.Parameters.AddWithValue("@trashRecycledCount", player.TrashRecycledCount);
        command.Parameters.AddWithValue("@revivesDone", player.RevivesDone);
        command.Parameters.AddWithValue("@timesFainted", player.TimesFainted);
        command.Parameters.AddWithValue("@durationSeconds", durationSeconds);
        command.Parameters.AddWithValue("@userId", player.UserId);

        var affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
        {
            throw new InvalidOperationException($"Player profile not found for user_id={player.UserId}.");
        }
    }

    private static async Task InsertCurrencyTransactionAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong matchId,
        MatchPlayerResultRequest player,
        uint balanceBefore,
        uint balanceAfter)
    {
        if (player.EarnedCurrency == 0)
            return;

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              INSERT INTO currency_transactions (
                                  user_id,
                                  transaction_type,
                                  amount,
                                  balance_before,
                                  balance_after,
                                  source_type,
                                  source_id,
                                  description
                              )
                              VALUES (
                                  @userId,
                                  'reward',
                                  @amount,
                                  @balanceBefore,
                                  @balanceAfter,
                                  'match',
                                  @matchId,
                                  'Match completion reward'
                              );
                              """;

        command.Parameters.AddWithValue("@userId", player.UserId);
        command.Parameters.AddWithValue("@amount", (int)player.EarnedCurrency);
        command.Parameters.AddWithValue("@balanceBefore", balanceBefore);
        command.Parameters.AddWithValue("@balanceAfter", balanceAfter);
        command.Parameters.AddWithValue("@matchId", matchId);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<uint> GetSoftCurrencyAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              SELECT soft_currency
                              FROM player_profiles
                              WHERE user_id = @userId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@userId", userId);

        var result = await command.ExecuteScalarAsync();

        if (result == null)
        {
            throw new InvalidOperationException($"Player profile not found for user_id={userId}.");
        }

        return Convert.ToUInt32(result);
    }
    
    private static async Task ConsumeUsedItemAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        UsedItemRequest usedItem)
    {
        ulong shopItemId = await GetUsableShopItemIdByCodeAsync(
            connection,
            transaction,
            usedItem.ItemCode
        );

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              UPDATE player_inventory
                              SET quantity = quantity - @quantity
                              WHERE user_id = @userId
                                AND shop_item_id = @shopItemId
                                AND quantity >= @quantity;
                              """;

        command.Parameters.AddWithValue("@quantity", usedItem.Quantity);
        command.Parameters.AddWithValue("@userId", usedItem.UserId);
        command.Parameters.AddWithValue("@shopItemId", shopItemId);

        var affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"Insufficient inventory quantity for user_id={usedItem.UserId}, item_code={usedItem.ItemCode}, quantity={usedItem.Quantity}."
            );
        }

        await DeleteZeroQuantityInventoryItemAsync(
            connection,
            transaction,
            usedItem.UserId,
            shopItemId
        );
    }
    private static async Task<ulong> GetUsableShopItemIdByCodeAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string itemCode)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              SELECT shop_item_id, item_type
                              FROM shop_items
                              WHERE item_code = @itemCode
                                AND is_active = TRUE
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@itemCode", itemCode);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Shop item not found or inactive. item_code={itemCode}.");

        ulong shopItemId = reader.GetUInt64("shop_item_id");
        string itemType = reader.GetString("item_type");

        if (itemType != "consumable" && itemType != "passive")
            throw new InvalidOperationException($"Item is not consumable. item_code={itemCode}, item_type={itemType}.");

        return shopItemId;
    }
    private static async Task DeleteZeroQuantityInventoryItemAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId,
        ulong shopItemId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              DELETE FROM player_inventory
                              WHERE user_id = @userId
                                AND shop_item_id = @shopItemId
                                AND quantity = 0;
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@shopItemId", shopItemId);

        await command.ExecuteNonQueryAsync();
    }
}