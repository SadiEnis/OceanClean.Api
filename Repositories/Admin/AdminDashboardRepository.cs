using OceanClean.Api.Data;
using OceanClean.Api.DTOs.Admin.Dashboard;

namespace OceanClean.Api.Repositories.Admin;

public class AdminDashboardRepository
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public AdminDashboardRepository(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT
                                  (SELECT COUNT(*) FROM users) AS total_players,
                                  (SELECT COUNT(*) FROM users WHERE player_status = 'active') AS active_players,
                                  (SELECT COUNT(*) FROM users WHERE player_status = 'banned') AS banned_players,
                                  (SELECT COUNT(*) FROM users WHERE player_status = 'inactive') AS inactive_players,

                                  (SELECT COUNT(*) FROM matches) AS total_matches,
                                  (SELECT COUNT(*) FROM player_action_logs) AS total_gameplay_events,

                                  COALESCE((
                                      SELECT SUM(amount)
                                      FROM currency_transactions
                                      WHERE amount > 0
                                  ), 0) AS total_currency_earned,

                                  COALESCE((
                                      SELECT SUM(ABS(amount))
                                      FROM currency_transactions
                                      WHERE amount < 0
                                  ), 0) AS total_currency_spent,

                                  COALESCE((
                                      SELECT SUM(amount)
                                      FROM currency_transactions
                                  ), 0) AS net_currency,

                                  (SELECT COUNT(*) FROM shop_purchase_logs) AS total_item_purchases,

                                  COALESCE((
                                      SELECT SUM(quantity)
                                      FROM shop_purchase_logs
                                  ), 0) AS total_purchased_quantity;
                              """;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return new AdminDashboardSummaryDto();

        return new AdminDashboardSummaryDto
        {
            TotalPlayers = Convert.ToInt64(reader["total_players"]),
            ActivePlayers = Convert.ToInt64(reader["active_players"]),
            BannedPlayers = Convert.ToInt64(reader["banned_players"]),
            InactivePlayers = Convert.ToInt64(reader["inactive_players"]),

            TotalMatches = Convert.ToInt64(reader["total_matches"]),
            TotalGameplayEvents = Convert.ToInt64(reader["total_gameplay_events"]),

            TotalCurrencyEarned = Convert.ToInt32(reader["total_currency_earned"]),
            TotalCurrencySpent = Convert.ToInt32(reader["total_currency_spent"]),
            NetCurrency = Convert.ToInt32(reader["net_currency"]),

            TotalItemPurchases = Convert.ToInt64(reader["total_item_purchases"]),
            TotalPurchasedQuantity = Convert.ToInt64(reader["total_purchased_quantity"])
        };
    }

    public async Task<List<AdminDashboardActivityPointDto>> GetActivityAsync(
    DateTime? from,
    DateTime? to,
    string bucketType)
{
    await using var connection = _connectionFactory.CreateConnection();
    await connection.OpenAsync();

    await using var command = connection.CreateCommand();

    string userCreatedBucket = BuildBucketExpression("u.created_at", bucketType);
    string userLoginBucket = BuildBucketExpression("pll.created_at", bucketType);
    string matchBucket = BuildBucketExpression("m.started_at", bucketType);
    string purchaseBucket = BuildBucketExpression("spl.purchased_at", bucketType);
    string currencyBucket = BuildBucketExpression("ct.created_at", bucketType);
    string actionLogBucket = BuildBucketExpression("pal.created_at", bucketType);

    var userCreatedWhere = new List<string>();
    var userLoginWhere = new List<string>();
    var matchWhere = new List<string>();
    var purchaseWhere = new List<string>();
    var currencyWhere = new List<string>();
    var actionLogWhere = new List<string>();

    if (from.HasValue)
    {
        userCreatedWhere.Add("u.created_at >= @from");
        userLoginWhere.Add("pll.created_at >= @from");
        matchWhere.Add("m.started_at >= @from");
        purchaseWhere.Add("spl.purchased_at >= @from");
        currencyWhere.Add("ct.created_at >= @from");
        actionLogWhere.Add("pal.created_at >= @from");

        command.Parameters.AddWithValue("@from", from.Value);
    }

    if (to.HasValue)
    {
        userCreatedWhere.Add("u.created_at <= @to");
        userLoginWhere.Add("pll.created_at <= @to");
        matchWhere.Add("m.started_at <= @to");
        purchaseWhere.Add("spl.purchased_at <= @to");
        currencyWhere.Add("ct.created_at <= @to");
        actionLogWhere.Add("pal.created_at <= @to");

        command.Parameters.AddWithValue("@to", to.Value);
    }

    string userCreatedWhereClause = BuildWhereClause(userCreatedWhere);
    string userLoginWhereClause = BuildWhereClause(userLoginWhere);
    string matchWhereClause = BuildWhereClause(matchWhere);
    string purchaseWhereClause = BuildWhereClause(purchaseWhere);
    string currencyWhereClause = BuildWhereClause(currencyWhere);
    string actionLogWhereClause = BuildWhereClause(actionLogWhere);

    command.CommandText = $"""
                           SELECT
                               combined.bucket,

                               SUM(combined.new_players) AS new_players,
                               0 AS total_players,
                               SUM(combined.player_logins) AS player_logins,
                               SUM(combined.matches_played) AS matches_played,
                               SUM(combined.item_purchases) AS item_purchases,
                               SUM(combined.gameplay_events) AS gameplay_events,
                               SUM(combined.currency_earned) AS currency_earned,
                               SUM(combined.currency_spent) AS currency_spent
                           FROM
                           (
                               SELECT
                                   {userCreatedBucket} AS bucket,
                                   COUNT(*) AS new_players,
                                   0 AS player_logins,
                                   0 AS matches_played,
                                   0 AS item_purchases,
                                   0 AS gameplay_events,
                                   0 AS currency_earned,
                                   0 AS currency_spent
                               FROM users u
                               {userCreatedWhereClause}
                               GROUP BY bucket

                               UNION ALL

                               SELECT
                                   {userLoginBucket} AS bucket,
                                   0 AS new_players,
                                   COUNT(*) AS player_logins,
                                   0 AS matches_played,
                                   0 AS item_purchases,
                                   0 AS gameplay_events,
                                   0 AS currency_earned,
                                   0 AS currency_spent
                               FROM player_login_logs pll
                               {userLoginWhereClause}
                               GROUP BY bucket

                               UNION ALL

                               SELECT
                                   {matchBucket} AS bucket,
                                   0 AS new_players,
                                   0 AS player_logins,
                                   COUNT(*) AS matches_played,
                                   0 AS item_purchases,
                                   0 AS gameplay_events,
                                   0 AS currency_earned,
                                   0 AS currency_spent
                               FROM matches m
                               {matchWhereClause}
                               GROUP BY bucket

                               UNION ALL

                               SELECT
                                   {purchaseBucket} AS bucket,
                                   0 AS new_players,
                                   0 AS player_logins,
                                   0 AS matches_played,
                                   COUNT(*) AS item_purchases,
                                   0 AS gameplay_events,
                                   0 AS currency_earned,
                                   0 AS currency_spent
                               FROM shop_purchase_logs spl
                               {purchaseWhereClause}
                               GROUP BY bucket

                               UNION ALL

                               SELECT
                                   {actionLogBucket} AS bucket,
                                   0 AS new_players,
                                   0 AS player_logins,
                                   0 AS matches_played,
                                   0 AS item_purchases,
                                   COUNT(*) AS gameplay_events,
                                   0 AS currency_earned,
                                   0 AS currency_spent
                               FROM player_action_logs pal
                               {actionLogWhereClause}
                               GROUP BY bucket

                               UNION ALL

                               SELECT
                                   {currencyBucket} AS bucket,
                                   0 AS new_players,
                                   0 AS player_logins,
                                   0 AS matches_played,
                                   0 AS item_purchases,
                                   0 AS gameplay_events,
                                   COALESCE(SUM(CASE WHEN ct.amount > 0 THEN ct.amount ELSE 0 END), 0) AS currency_earned,
                                   COALESCE(SUM(CASE WHEN ct.amount < 0 THEN ABS(ct.amount) ELSE 0 END), 0) AS currency_spent
                               FROM currency_transactions ct
                               {currencyWhereClause}
                               GROUP BY bucket
                           ) combined
                           WHERE combined.bucket IS NOT NULL
                           GROUP BY combined.bucket
                           ORDER BY combined.bucket ASC;
                           """;

    var points = new List<AdminDashboardActivityPointDto>();

    await using (var reader = await command.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            points.Add(new AdminDashboardActivityPointDto
            {
                Bucket = reader.GetString("bucket"),
                NewPlayers = Convert.ToInt64(reader["new_players"]),
                TotalPlayers = 0,
                PlayerLogins = Convert.ToInt64(reader["player_logins"]),
                MatchesPlayed = Convert.ToInt64(reader["matches_played"]),
                ItemPurchases = Convert.ToInt64(reader["item_purchases"]),
                GameplayEvents = Convert.ToInt64(reader["gameplay_events"]),
                CurrencyEarned = Convert.ToInt32(reader["currency_earned"]),
                CurrencySpent = Convert.ToInt32(reader["currency_spent"])
            });
        }
    }

    await FillCumulativeTotalPlayersAsync(connection, points, bucketType);

    return points;
}

    private static async Task FillCumulativeTotalPlayersAsync(
        MySqlConnector.MySqlConnection connection,
        List<AdminDashboardActivityPointDto> points,
        string bucketType)
    {
        if (points.Count == 0)
            return;

        string bucketFormat = bucketType switch
        {
            "hour" => "%Y-%m-%d %H:00",
            "day" => "%Y-%m-%d",
            "month" => "%Y-%m",
            _ => "%Y-%m-%d"
        };

        foreach (var point in points)
        {
            await using var command = connection.CreateCommand();

            command.CommandText = """
                                  SELECT COUNT(*)
                                  FROM users
                                  WHERE DATE_FORMAT(created_at, @bucketFormat) <= @bucket;
                                  """;

            command.Parameters.AddWithValue("@bucketFormat", bucketFormat);
            command.Parameters.AddWithValue("@bucket", point.Bucket);

            object? result = await command.ExecuteScalarAsync();

            point.TotalPlayers = result == null || result == DBNull.Value
                ? 0
                : Convert.ToInt64(result);
        }
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

    private static string BuildWhereClause(List<string> whereClauses)
    {
        return whereClauses.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", whereClauses);
    }
}