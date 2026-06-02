namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyItemSummaryDto
{
    public ulong ShopItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public uint TotalQuantity { get; set; }
    public uint EventCount { get; set; }

    public int TotalCurrencyAmount { get; set; }
}