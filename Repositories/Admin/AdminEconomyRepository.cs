using MySqlConnector;
using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.Economy;

namespace OceanClean.Api.Repositories.Admin;

public class AdminEconomyRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminEconomyRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<AdminEconomyItemSummaryDto>> GetTopPurchasedItemsAsync(
        DateTime? from,
        DateTime? to,
        int limit)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClause = BuildPurchaseDateWhereClause(command, from, to);

        command.CommandText = $"""
                               SELECT
                                   si.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type,
                                   COALESCE(SUM(spl.quantity), 0) AS total_quantity,
                                   COUNT(spl.purchase_log_id) AS event_count,
                                   COALESCE(SUM(spl.total_price), 0) AS total_currency_amount
                               FROM shop_purchase_logs spl
                               JOIN shop_items si ON si.shop_item_id = spl.shop_item_id
                               {whereClause}
                               GROUP BY
                                   si.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type
                               ORDER BY total_quantity DESC, total_currency_amount DESC
                               LIMIT @limit;
                               """;

        command.Parameters.AddWithValue("@limit", Math.Clamp(limit, 1, 50));

        var items = new List<AdminEconomyItemSummaryDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminEconomyItemSummaryDto
            {
                ShopItemId = reader.GetUInt64("shop_item_id"),
                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemType = reader.GetString("item_type"),
                TotalQuantity = Convert.ToUInt32(reader["total_quantity"]),
                EventCount = Convert.ToUInt32(reader["event_count"]),
                TotalCurrencyAmount = Convert.ToInt32(reader["total_currency_amount"])
            });
        }

        return items;
    }

    public async Task<List<AdminEconomyItemSummaryDto>> GetTopUsedItemsAsync(
        DateTime? from,
        DateTime? to,
        int limit)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string whereClause = BuildUsedItemDateWhereClause(command, from, to);

        command.CommandText = $"""
                               SELECT
                                   si.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type,
                                   COALESCE(SUM(COALESCE(pal.value, 0)), 0) AS total_quantity,
                                   COUNT(pal.log_id) AS event_count,
                                   0 AS total_currency_amount
                               FROM player_action_logs pal
                               JOIN shop_items si ON si.shop_item_id = pal.shop_item_id
                               {whereClause}
                               GROUP BY
                                   si.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type
                               ORDER BY total_quantity DESC, event_count DESC
                               LIMIT @limit;
                               """;

        command.Parameters.AddWithValue("@limit", Math.Clamp(limit, 1, 50));

        var items = new List<AdminEconomyItemSummaryDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminEconomyItemSummaryDto
            {
                ShopItemId = reader.GetUInt64("shop_item_id"),
                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemType = reader.GetString("item_type"),
                TotalQuantity = Convert.ToUInt32(reader["total_quantity"]),
                EventCount = Convert.ToUInt32(reader["event_count"]),
                TotalCurrencyAmount = Convert.ToInt32(reader["total_currency_amount"])
            });
        }

        return items;
    }

    private static string BuildPurchaseDateWhereClause(
        MySqlCommand command,
        DateTime? from,
        DateTime? to)
    {
        var where = new List<string>();

        if (from.HasValue)
        {
            where.Add("spl.purchased_at >= @purchaseFrom");
            command.Parameters.AddWithValue("@purchaseFrom", from.Value);
        }

        if (to.HasValue)
        {
            where.Add("spl.purchased_at <= @purchaseTo");
            command.Parameters.AddWithValue("@purchaseTo", to.Value);
        }

        return where.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", where);
    }

    private static string BuildUsedItemDateWhereClause(
        MySqlCommand command,
        DateTime? from,
        DateTime? to)
    {
        var where = new List<string>
        {
            "pal.action_type = 'use_item'",
            "pal.shop_item_id IS NOT NULL"
        };

        if (from.HasValue)
        {
            where.Add("pal.created_at >= @usedFrom");
            command.Parameters.AddWithValue("@usedFrom", from.Value);
        }

        if (to.HasValue)
        {
            where.Add("pal.created_at <= @usedTo");
            command.Parameters.AddWithValue("@usedTo", to.Value);
        }

        return "WHERE " + string.Join(" AND ", where);
    }
}