namespace OceanClean.Api.DTOs.Admin.Dashboard;

public class AdminDashboardActivityPointDto
{
    public string Bucket { get; set; } = string.Empty;

    public long NewPlayers { get; set; }
    public long PlayerLogins { get; set; }
    public long MatchesPlayed { get; set; }
    public long ItemPurchases { get; set; }
    public long GameplayEvents { get; set; }

    public int CurrencyEarned { get; set; }
    public int CurrencySpent { get; set; }
}