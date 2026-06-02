namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerInventoryItemDto
{
    public ulong InventoryId { get; set; }
    public ulong ShopItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public uint Quantity { get; set; }
    public DateTime AcquiredAt { get; set; }
}