namespace OceanClean.Api.DTOs.Admin.Dashboard;

public class AdminDashboardSummaryDto
{
    public long TotalPlayers { get; set; }
    public long ActivePlayers { get; set; }
    public long BannedPlayers { get; set; }
    public long InactivePlayers { get; set; }

    public long TotalMatches { get; set; }
    public long TotalGameplayEvents { get; set; }

    public int TotalCurrencyEarned { get; set; }
    public int TotalCurrencySpent { get; set; }
    public int NetCurrency { get; set; }

    public long TotalItemPurchases { get; set; }
    public long TotalPurchasedQuantity { get; set; }
}