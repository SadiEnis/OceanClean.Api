using OceanClean.Api.Data;
using OceanClean.Api.Models.Inventory;

namespace OceanClean.Api.Repositories;

public class InventoryRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public InventoryRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<InventoryItemRecord>> GetInventoryByUserIdAsync(ulong userId)
    {
        var items = new List<InventoryItemRecord>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  pi.inventory_id,
                                  si.shop_item_id,
                                  si.item_code,
                                  si.item_name,
                                  si.item_type,
                                  pi.quantity,
                                  si.max_quantity
                              FROM player_inventory pi
                              INNER JOIN shop_items si ON si.shop_item_id = pi.shop_item_id
                              WHERE pi.user_id = @userId
                              ORDER BY si.shop_item_id;
                              """;

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new InventoryItemRecord
            {
                InventoryId = reader.GetUInt64("inventory_id"),
                ShopItemId = reader.GetUInt64("shop_item_id"),

                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemType = reader.GetString("item_type"),

                Quantity = reader.GetUInt32("quantity"),
                MaxQuantity = reader.GetUInt32("max_quantity")
            });
        }

        return items;
    }
}