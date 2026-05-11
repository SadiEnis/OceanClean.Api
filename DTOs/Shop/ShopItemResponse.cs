namespace OceanClean.Api.DTOs.Shop;

public class ShopItemResponse
{
    public ulong ShopItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public uint PriceSoftCurrency { get; set; }
    public uint MaxQuantity { get; set; }

    public bool IsActive { get; set; }
}