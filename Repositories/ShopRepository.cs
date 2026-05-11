using OceanClean.Api.Data;
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
}