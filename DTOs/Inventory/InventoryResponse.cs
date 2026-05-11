namespace OceanClean.Api.DTOs.Inventory;

public class InventoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public ulong UserId { get; set; }

    public List<InventoryItemResponse> Items { get; set; } = new();
}