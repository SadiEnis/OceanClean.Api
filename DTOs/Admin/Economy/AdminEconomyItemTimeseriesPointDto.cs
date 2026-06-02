namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyItemTimeseriesPointDto
{
    public string Bucket { get; set; } = string.Empty;

    public ulong ShopItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public uint PurchasedQuantity { get; set; }
    public uint UsedQuantity { get; set; }
}