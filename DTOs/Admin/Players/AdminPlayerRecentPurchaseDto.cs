namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerRecentPurchaseDto
{
    public ulong PurchaseLogId { get; set; }

    public ulong ShopItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public uint Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int TotalPrice { get; set; }

    public int CurrencyBefore { get; set; }
    public int CurrencyAfter { get; set; }

    public DateTime PurchasedAt { get; set; }
}