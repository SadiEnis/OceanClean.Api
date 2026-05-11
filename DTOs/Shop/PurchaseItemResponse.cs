namespace OceanClean.Api.DTOs.Shop;

public class PurchaseItemResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public ulong UserId { get; set; }
    public ulong ShopItemId { get; set; }

    public uint PurchasedQuantity { get; set; }
    public uint NewBalance { get; set; }
}