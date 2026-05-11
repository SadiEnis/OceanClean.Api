using System.Data.Common;
using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Shop;
using OceanClean.Api.Models.Shop;

namespace OceanClean.Api.Repositories;

public class ShopRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public ShopRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ShopItemRecord>> GetActiveShopItemsAsync()
    {
        var items = new List<ShopItemRecord>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  shop_item_id,
                                  item_code,
                                  item_name,
                                  item_description,
                                  item_type,
                                  price_soft_currency,
                                  max_quantity,
                                  is_active
                              FROM shop_items
                              WHERE is_active = TRUE
                              ORDER BY shop_item_id;
                              """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new ShopItemRecord
            {
                ShopItemId = reader.GetUInt64("shop_item_id"),
                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemDescription = reader.IsDBNull(reader.GetOrdinal("item_description"))
                    ? null
                    : reader.GetString("item_description"),
                ItemType = reader.GetString("item_type"),
                PriceSoftCurrency = reader.GetUInt32("price_soft_currency"),
                MaxQuantity = reader.GetUInt32("max_quantity"),
                IsActive = reader.GetBoolean("is_active")
            });
        }

        return items;
    }

    public async Task<PurchaseItemResponse> PurchaseItemAsync(PurchaseItemRequest request)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var item = await GetShopItemForPurchaseAsync(connection, transaction, request.ShopItemId);

            if (item == null)
            {
                return await RollbackWithResponseAsync(
                    transaction,
                    "Shop item not found.",
                    request.UserId,
                    request.ShopItemId
                );
            }

            if (!item.IsActive)
            {
                return await RollbackWithResponseAsync(
                    transaction,
                    "Shop item is not active.",
                    request.UserId,
                    request.ShopItemId
                );
            }

            var currentQuantity = await GetCurrentInventoryQuantityAsync(
                connection,
                transaction,
                request.UserId,
                request.ShopItemId
            );

            if (currentQuantity + request.Quantity > item.MaxQuantity)
            {
                return await RollbackWithResponseAsync(
                    transaction,
                    $"Max quantity limit exceeded. MaxQuantity={item.MaxQuantity}, CurrentQuantity={currentQuantity}.",
                    request.UserId,
                    request.ShopItemId
                );
            }

            var balanceBefore = await GetSoftCurrencyAsync(connection, transaction, request.UserId);

            var totalPrice = item.PriceSoftCurrency * request.Quantity;

            if (balanceBefore < totalPrice)
            {
                return await RollbackWithResponseAsync(
                    transaction,
                    "Insufficient soft currency.",
                    request.UserId,
                    request.ShopItemId
                );
            }

            var balanceAfter = balanceBefore - totalPrice;

            await DecreaseSoftCurrencyAsync(connection, transaction, request.UserId, totalPrice);
            await UpsertInventoryItemAsync(connection, transaction, request.UserId, request.ShopItemId,
                request.Quantity);

            await InsertPurchaseCurrencyTransactionAsync(
                connection,
                transaction,
                request.UserId,
                request.ShopItemId,
                totalPrice,
                balanceBefore,
                balanceAfter,
                item.ItemCode
            );

            await transaction.CommitAsync();

            return new PurchaseItemResponse
            {
                Success = true,
                Message = "Purchase completed successfully.",
                UserId = request.UserId,
                ShopItemId = request.ShopItemId,
                PurchasedQuantity = request.Quantity,
                NewBalance = balanceAfter
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // Class Helpers
    private static async Task<PurchaseItemResponse> RollbackWithResponseAsync(
        MySqlTransaction transaction,
        string message,
        ulong userId,
        ulong shopItemId)
    {
        await transaction.RollbackAsync();

        return new PurchaseItemResponse
        {
            Success = false,
            Message = message,
            UserId = userId,
            ShopItemId = shopItemId
        };
    }
    
    private static async Task<PurchaseShopItemRecord?> GetShopItemForPurchaseAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong shopItemId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              SELECT
                                  shop_item_id,
                                  item_code,
                                  item_name,
                                  item_type,
                                  price_soft_currency,
                                  max_quantity,
                                  is_active
                              FROM shop_items
                              WHERE shop_item_id = @shopItemId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@shopItemId", shopItemId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new PurchaseShopItemRecord
        {
            ShopItemId = reader.GetUInt64("shop_item_id"),
            ItemCode = reader.GetString("item_code"),
            ItemName = reader.GetString("item_name"),
            ItemType = reader.GetString("item_type"),
            PriceSoftCurrency = reader.GetUInt32("price_soft_currency"),
            MaxQuantity = reader.GetUInt32("max_quantity"),
            IsActive = reader.GetBoolean("is_active")
        };
    }
    
    private static async Task<uint> GetCurrentInventoryQuantityAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId,
        ulong shopItemId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              SELECT quantity
                              FROM player_inventory
                              WHERE user_id = @userId
                                AND shop_item_id = @shopItemId
                              LIMIT 1;
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@shopItemId", shopItemId);

        var result = await command.ExecuteScalarAsync();

        if (result == null)
            return 0;

        return Convert.ToUInt32(result);
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
            throw new InvalidOperationException($"Player profile not found for user_id={userId}.");

        return Convert.ToUInt32(result);
    }
    
    private static async Task DecreaseSoftCurrencyAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId,
        uint amount)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              UPDATE player_profiles
                              SET soft_currency = soft_currency - @amount
                              WHERE user_id = @userId;
                              """;

        command.Parameters.AddWithValue("@amount", amount);
        command.Parameters.AddWithValue("@userId", userId);

        var affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
            throw new InvalidOperationException($"Player profile not found for user_id={userId}.");
    }
    
    private static async Task UpsertInventoryItemAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId,
        ulong shopItemId,
        uint quantity)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
                              INSERT INTO player_inventory (
                                  user_id,
                                  shop_item_id,
                                  quantity
                              )
                              VALUES (
                                  @userId,
                                  @shopItemId,
                                  @quantity
                              )
                              ON DUPLICATE KEY UPDATE
                                  quantity = quantity + VALUES(quantity);
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@shopItemId", shopItemId);
        command.Parameters.AddWithValue("@quantity", quantity);

        await command.ExecuteNonQueryAsync();
    }
    
    private static async Task InsertPurchaseCurrencyTransactionAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        ulong userId,
        ulong shopItemId,
        uint totalPrice,
        uint balanceBefore,
        uint balanceAfter,
        string itemCode)
    {
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
                                  'purchase',
                                  @amount,
                                  @balanceBefore,
                                  @balanceAfter,
                                  'shop_item',
                                  @shopItemId,
                                  @description
                              );
                              """;

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@amount", -(int)totalPrice);
        command.Parameters.AddWithValue("@balanceBefore", balanceBefore);
        command.Parameters.AddWithValue("@balanceAfter", balanceAfter);
        command.Parameters.AddWithValue("@shopItemId", shopItemId);
        command.Parameters.AddWithValue("@description", $"Purchased item: {itemCode}");

        await command.ExecuteNonQueryAsync();
    }
}