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

    public async Task<List<AdminEconomyItemTimeseriesPointDto>> GetItemTimeseriesAsync(
        DateTime? from,
        DateTime? to,
        string bucketType)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        string purchaseBucketExpression = BuildBucketExpression("spl.purchased_at", bucketType);
        string usageBucketExpression = BuildBucketExpression("pal.created_at", bucketType);

        var purchaseWhere = new List<string>();
        var usageWhere = new List<string>
        {
            "pal.action_type = 'use_item'",
            "pal.shop_item_id IS NOT NULL"
        };

        if (from.HasValue)
        {
            purchaseWhere.Add("spl.purchased_at >= @purchaseFrom");
            usageWhere.Add("pal.created_at >= @usageFrom");

            command.Parameters.AddWithValue("@purchaseFrom", from.Value);
            command.Parameters.AddWithValue("@usageFrom", from.Value);
        }

        if (to.HasValue)
        {
            purchaseWhere.Add("spl.purchased_at <= @purchaseTo");
            usageWhere.Add("pal.created_at <= @usageTo");

            command.Parameters.AddWithValue("@purchaseTo", to.Value);
            command.Parameters.AddWithValue("@usageTo", to.Value);
        }

        string purchaseWhereClause = purchaseWhere.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", purchaseWhere);

        string usageWhereClause = "WHERE " + string.Join(" AND ", usageWhere);

        command.CommandText = $"""
                               SELECT
                                   combined.bucket,
                                   combined.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type,
                                   SUM(combined.purchased_quantity) AS purchased_quantity,
                                   SUM(combined.used_quantity) AS used_quantity
                               FROM
                               (
                                   SELECT
                                       {purchaseBucketExpression} AS bucket,
                                       spl.shop_item_id,
                                       SUM(spl.quantity) AS purchased_quantity,
                                       0 AS used_quantity
                                   FROM shop_purchase_logs spl
                                   {purchaseWhereClause}
                                   GROUP BY bucket, spl.shop_item_id

                                   UNION ALL

                                   SELECT
                                       {usageBucketExpression} AS bucket,
                                       pal.shop_item_id,
                                       0 AS purchased_quantity,
                                       SUM(COALESCE(pal.value, 0)) AS used_quantity
                                   FROM player_action_logs pal
                                   {usageWhereClause}
                                   GROUP BY bucket, pal.shop_item_id
                               ) combined
                               JOIN shop_items si ON si.shop_item_id = combined.shop_item_id
                               GROUP BY
                                   combined.bucket,
                                   combined.shop_item_id,
                                   si.item_code,
                                   si.item_name,
                                   si.item_type
                               ORDER BY combined.bucket ASC, si.item_code ASC;
                               """;

        var points = new List<AdminEconomyItemTimeseriesPointDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            points.Add(new AdminEconomyItemTimeseriesPointDto
            {
                Bucket = reader.GetString("bucket"),
                ShopItemId = reader.GetUInt64("shop_item_id"),
                ItemCode = reader.GetString("item_code"),
                ItemName = reader.GetString("item_name"),
                ItemType = reader.GetString("item_type"),
                PurchasedQuantity = Convert.ToUInt32(reader["purchased_quantity"]),
                UsedQuantity = Convert.ToUInt32(reader["used_quantity"])
            });
        }

        return points;
    }

    private static string BuildBucketExpression(string dateColumn, string bucketType)
    {
        return bucketType switch
        {
            "hour" => $"DATE_FORMAT({dateColumn}, '%Y-%m-%d %H:00')",
            "day" => $"DATE_FORMAT({dateColumn}, '%Y-%m-%d')",
            "month" => $"DATE_FORMAT({dateColumn}, '%Y-%m')",
            _ => $"DATE_FORMAT({dateColumn}, '%Y-%m-%d')"
        };
    }
}