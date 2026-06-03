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
}