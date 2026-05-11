namespace OceanClean.Api.Models.Shop;

public class PurchaseShopItemRecord
{
    public ulong ShopItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public uint PriceSoftCurrency { get; set; }
    public uint MaxQuantity { get; set; }

    public bool IsActive { get; set; }
}