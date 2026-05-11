namespace OceanClean.Api.DTOs.Shop;

public class PurchaseItemRequest
{
    public ulong UserId { get; set; }
    public ulong ShopItemId { get; set; }
    public uint Quantity { get; set; } = 1;
}